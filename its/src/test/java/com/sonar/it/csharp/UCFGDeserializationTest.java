/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2018 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.OrchestratorBuilder;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.container.Edition;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import com.sonar.orchestrator.locator.MavenLocation;
import com.sonar.orchestrator.util.Command;
import com.sonar.orchestrator.util.CommandExecutor;
import com.sonar.orchestrator.util.StreamConsumer;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import org.junit.BeforeClass;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.ucfg.UCFG;
import org.sonar.ucfg.UCFGtoProtobuf;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.fail;

// FIXME must remove this test after the 7.4 release
public class UCFGDeserializationTest {
  private static final String PROJECT_KEY = "ucfg_tests"; // same key used for different MSBuild projects
  private static final String QUALITY_PROFILE = "CsharpProfile";

  private static Orchestrator orchestrator;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @BeforeClass
  public static void initializeOrchestrator() {
    // Versions of SonarQube and plugins support aliases:
    // - "DEV" for the latest build of master that passed QA
    // - "DEV[1.0]" for the latest build that passed QA of series 1.0.x
    // - "LATEST_RELEASE" for the latest release
    // - "LATEST_RELEASE[1.0]" for latest release of series 1.0.x
    // The SonarQube alias "LTS" has been dropped. An alternative is "LATEST_RELEASE[6.7]".
    // The term "latest" refers to the highest version number, not the most recently published version.

    String sonarVersion = Optional.ofNullable(System.getProperty("sonar.runtimeVersion")).filter(v -> !"LTS".equals(v)).orElse("LATEST_RELEASE[6.7]");

    // Starting with SonarQube 7.4, the responsibility for handling UCFGs moves to Sonar Security C# Frontend Plugin,
    // breaking compatibility with this plugin. We keep executing these tests only for previous versions.
    if (sonarVersion.contains("DEV") || sonarVersion.contains("7.4")) {
      return;
    }

    OrchestratorBuilder builder = Orchestrator.builderEnv()
      .setSonarVersion(sonarVersion)
      .setEdition(Edition.DEVELOPER)
      .addPlugin(TestUtils.getPluginLocation("sonar-csharp-plugin"))
      .addPlugin(MavenLocation.of("com.sonarsource.security", "sonar-security-plugin", "7.3.0.1282"))
      .activateLicense();

    orchestrator = builder.build();
    orchestrator.start();

    orchestrator.getServer().provisionProject(PROJECT_KEY, PROJECT_KEY);
  }

  private static void createQP(String ... ruleKeys) throws IOException {
    String cSharpProfile = profile("cs", "csharpsquid", ruleKeys);
    File file = File.createTempFile("profile", ".xml");
    Files.write(file.toPath(), cSharpProfile.getBytes());

    orchestrator.getServer().restoreProfile(FileLocation.of(file));
    orchestrator.getServer().associateProjectToQualityProfile(PROJECT_KEY, "cs", QUALITY_PROFILE);

    file.delete();
  }

  private static String profile(String language, String repositoryKey, String ... ruleKeys) {
    StringBuilder sb = new StringBuilder()
      .append("<profile>")
      .append("<name>").append(QUALITY_PROFILE).append("</name>")
      .append("<language>").append(language).append("</language>")
      .append("<rules>");
    Arrays.stream(ruleKeys).forEach(ruleKey -> {
      sb.append("<rule>")
        .append("<repositoryKey>").append(repositoryKey).append("</repositoryKey>")
        .append("<key>").append(ruleKey).append("</key>")
        .append("<priority>INFO</priority>")
        .append("</rule>");
    });
    return sb
      .append("</rules>")
      .append("</profile>")
      .toString();
  }

  @Test
  public void ucfgs_created_when_rules_enabled() throws IOException {
    if (orchestrator == null) {
      return;
    }
    // enable a security rule
    createQP("S3649");

    Path projectDir = Tests.projectDir(temp, "SimplCommerce");
    runAnalysis(projectDir);

    List<UCFG> ucfgs = readUcfgs(projectDir);
    assertThat(ucfgs).hasSize(926);
  }

  @Test
  public void ucfgs_not_created_when_rules_not_enabled() throws IOException {
    if (orchestrator == null) {
      return;
    }
    // No security rules in QP
    createQP("S100");

    Path projectDir = Tests.projectDir(temp, "UcfgNoProtobuf");
    runAnalysis(projectDir);

    File csharpDir = new File(projectDir.toFile(), ".sonarqube/out/ucfg_cs");
    assertThat(csharpDir.exists()).isFalse();
  }

  private static List<UCFG> readUcfgs(Path projectDir) {
    File csharpDir = new File(projectDir.toFile(), ".sonarqube/out/ucfg_cs");
    List<UCFG> result = new ArrayList<>();
    if (csharpDir.isDirectory()) {
      try {
        for (File file : csharpDir.listFiles()) {
          result.add(UCFGtoProtobuf.fromProtobufFile(file));
        }
      } catch (Exception | Error ioe) {
        fail("An error occured while deserializing ucfgs : ", ioe);
      }
    } else {
      fail("Did not find ucfgs directory at " + csharpDir.getAbsolutePath());
    }
    return result;
  }

  private void runAnalysis(Path projectDir) {
    orchestrator.executeBuild(getScannerForMSBuild(projectDir)
      .addArgument("begin")
      .setDebugLogs(true)
      .setProjectKey(PROJECT_KEY)
      .setProjectName(PROJECT_KEY)
      .setProjectVersion("1.0"));

    executeDotNetBuild(projectDir, "build");

    orchestrator.executeBuild(getScannerForMSBuild(projectDir).addArgument("end"));
  }

  private static ScannerForMSBuild getScannerForMSBuild(Path projectDir) {
    return ScannerForMSBuild.create()
      .setScannerVersion("4.3.1.1372")
      .setUseDotNetCore(true)
      .setProjectDir(projectDir.toFile());
  }

  private void executeDotNetBuild(Path projectDir, String... arguments) {
    BuildResult result = new BuildResult();
    StreamConsumer.Pipe writer = new StreamConsumer.Pipe(result.getLogsWriter());
    int status = CommandExecutor.create().execute(Command.create("dotnet")
      .addArguments(arguments)
      .setDirectory(projectDir.toFile()), writer, 10 * 60 * 1000);
    result.addStatus(status);

    assertThat(result.getLogs()).as("Roslyn analyzer errors").doesNotContain("AD0001");
    assertThat(result.isSuccess()).isTrue();
  }
}
