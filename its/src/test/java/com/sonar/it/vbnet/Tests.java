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
package com.sonar.it.vbnet;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.Build;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.container.Edition;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import com.sonar.orchestrator.locator.MavenLocation;
import com.sonar.orchestrator.util.Command;
import com.sonar.orchestrator.util.CommandExecutor;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.Optional;
import javax.annotation.CheckForNull;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.junit.ClassRule;
import org.junit.rules.TemporaryFolder;
import org.junit.runner.RunWith;
import org.junit.runners.Suite;
import org.junit.runners.Suite.SuiteClasses;
import org.sonarqube.ws.WsComponents;
import org.sonarqube.ws.WsMeasures;
import org.sonarqube.ws.WsMeasures.Measure;
import org.sonarqube.ws.client.HttpConnector;
import org.sonarqube.ws.client.WsClient;
import org.sonarqube.ws.client.WsClientFactories;
import org.sonarqube.ws.client.component.ShowWsRequest;
import org.sonarqube.ws.client.measure.ComponentWsRequest;

import static java.util.Collections.singletonList;
import static java.util.Objects.requireNonNull;
import static org.assertj.core.api.Assertions.assertThat;

@RunWith(Suite.class)
@SuiteClasses({
  CoverageTest.class,
  DoNotAnalyzeTestFilesTest.class,
  MetricsTest.class,
  NoSonarTest.class,
  ScannerPluginCompatibilityTest.class,
  UnitTestResultsTest.class
})
public class Tests {

  private static final String MSBUILD_PATH = "MSBUILD_PATH";

  @ClassRule
  public static final Orchestrator ORCHESTRATOR = Orchestrator.builderEnv()
    .setSonarVersion(Optional.ofNullable(System.getProperty("sonar.runtimeVersion")).filter(v -> !"LTS".equals(v)).orElse("LATEST_RELEASE[6.7]"))
    .addPlugin(getVbNetLocation())
    .restoreProfileAtStartup(FileLocation.of("profiles/vbnet_no_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/vbnet_class_name.xml"))
    .build();

  public static Location getVbNetLocation () {
    Location vbnetLocation;
    String csharpVersion = System.getProperty("csharpVersion"); // C# and VB.Net versions are the same
    if (StringUtils.isEmpty(csharpVersion)) {
      // use the plugin that was built on local machine
      vbnetLocation = FileLocation.byWildcardMavenFilename(new File("../sonar-vbnet-plugin/target"), "sonar-vbnet-plugin-*.jar");
    } else {
      // QA environment downloads the plugin built by the CI job
      vbnetLocation = MavenLocation.of("org.sonarsource.dotnet", "sonar-vbnet-plugin", csharpVersion);
    }
    return vbnetLocation;
  }

  public static Path projectDir(TemporaryFolder temp, String projectName) throws IOException {
    Path projectDir = Paths.get("projects").resolve(projectName);
    FileUtils.deleteDirectory(new File(temp.getRoot(), projectName));
    Path tmpProjectDir = temp.newFolder(projectName).toPath();
    FileUtils.copyDirectory(projectDir.toFile(), tmpProjectDir.toFile());
    return tmpProjectDir;
  }

  public static Build<ScannerForMSBuild> newScanner(Path projectDir) {
    return ScannerForMSBuild.create(projectDir.toFile())
      .setScannerVersion(System.getProperty("scannerMsbuild.version"));
  }

  public static void runMSBuild(Orchestrator orch, Path projectDir, String... arguments) {
    Path msBuildPath = getMsBuildPath(orch);

    int r = CommandExecutor.create().execute(Command.create(msBuildPath.toString())
      .addArguments(arguments)
      .setDirectory(projectDir.toFile()), 60 * 1000);
    assertThat(r).isEqualTo(0);
  }

  private static Path getMsBuildPath(Orchestrator orch) {
    String msBuildPathStr = orch.getConfiguration().getString(MSBUILD_PATH, "C:\\Program Files (x86)\\"
      + "Microsoft Visual Studio\\2017\\Enterprise\\MSBuild\\15.0\\Bin\\MSBuild.exe");
    Path msBuildPath = Paths.get(msBuildPathStr).toAbsolutePath();
    if (!Files.exists(msBuildPath)) {
      throw new IllegalStateException("Unable to find MSBuild at '" + msBuildPath.toString() +
        "'. Please configure property '" + MSBUILD_PATH + "'");
    }
    return msBuildPath;
  }

  @CheckForNull
  static Integer getMeasureAsInt(String componentKey, String metricKey) {
    Measure measure = getMeasure(componentKey, metricKey);
    return (measure == null) ? null : Integer.parseInt(measure.getValue());
  }

  @CheckForNull
  static Measure getMeasure(String componentKey, String metricKey) {
    WsMeasures.ComponentWsResponse response = newWsClient().measures().component(new ComponentWsRequest()
      .setComponentKey(componentKey)
      .setMetricKeys(singletonList(metricKey)));
    List<Measure> measures = response.getComponent().getMeasuresList();
    return measures.size() == 1 ? measures.get(0) : null;
  }

  static WsClient newWsClient() {
    return WsClientFactories.getDefault().newClient(HttpConnector.newBuilder()
      .url(ORCHESTRATOR.getServer().getUrl())
      .build());
  }

  static WsComponents.Component getComponent(String componentKey) {
    return newWsClient().components().show(new ShowWsRequest().setKey(componentKey)).getComponent();
  }

}
