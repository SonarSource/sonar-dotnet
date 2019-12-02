/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2019 SonarSource SA
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
import com.sonar.it.shared.VstsUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Ignore;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getMeasure;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class CoverageTest {


  final private static Logger LOG = LoggerFactory.getLogger(CoverageTest.class);

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @ClassRule
  public static final TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() {
    orchestrator.resetData();
  }

  @Test
  @Ignore // FIXME
  public void should_not_import_coverage_without_report() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject();

    assertThat(buildResult.getLogs())
      .doesNotContain("Sensor C# Tests Coverage Report Import")
      .doesNotContain("Coverage Report Statistics:");

    org.sonarqube.ws.Measures.Measure linesToCover = getMeasure("CoverageTest", "lines_to_cover");
    org.sonarqube.ws.Measures.Measure uncoveredLines = getMeasure("CoverageTest", "uncovered_lines");

    assertThat(linesToCover.getValue()).isEqualTo("2");
    assertThat(uncoveredLines.getValue()).isEqualTo("2");
}

  @Test
  public void ncover3() throws Exception {
    if (VstsUtils.isRunningUnderVsts()) {
      LOG.info("ncover3 running in VSTS  - will enumerate files");
      String vstsSourcePath = VstsUtils.getSourcesDirectory();
      LOG.info("TEST RUN: Tests are running under VSTS. Build dir:  " + vstsSourcePath);
      LOG.info("TEST RUN: Will enumerate files in the build directory");
      File baseDirectory = new File(vstsSourcePath);
      try (Stream<Path> walk = Files.walk(Paths.get(baseDirectory.toURI()))) {

        List<String> result = walk
          .map(Path::toString)
          .collect(Collectors.toList());

        result.forEach(LOG::info);

      } catch (IOException e) {
        e.printStackTrace();
      }
      LOG.info("TEST RUN: Tests are running under VSTS. Temp dir:  " + temp.getRoot().getAbsolutePath());
      LOG.info("TEST RUN: Will enumerate files in the temp directory");
      try (Stream<Path> walk = Files.walk(Paths.get(temp.getRoot().getAbsolutePath()))) {

        List<String> result = walk
          .map(Path::toString)
          .collect(Collectors.toList());

        result.forEach(LOG::info);

      } catch (IOException e) {
        e.printStackTrace();
      }
    }
    else {
      LOG.warn("NOT RUNNING IN VSTS ncover3");
    }

    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.ncover3.reportsPaths", "reports/ncover3.nccov");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  @Ignore // FIXME
  public void open_cover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.opencover.reportsPaths", "reports/opencover.xml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(0);
  }

  @Test
  @Ignore // FIXME
  public void dotcover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.dotcover.reportsPaths", "reports/dotcover.html");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  @Ignore // FIXME
  public void visual_studio() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  @Ignore // FIXME
  public void no_coverage_on_tests() throws Exception {
    Path projectDir = Tests.projectDir(temp, "NoCoverageOnTests");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("NoCoverageOnTests")
      .setProjectVersion("1.0")
      .setProfile("no_rule")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    BuildResult buildResult = orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 0 main files, 0 main files with coverage, 1 test files, 0 project excluded files, 0 other language files.",
      "WARN: The Code Coverage report doesn't contain any coverage data for the included files.");

    assertThat(getMeasureAsInt("NoCoverageOnTests", "files")).isEqualTo(2); // Only main files are counted
    String unitTestComponentId = TestUtils.hasModules(ORCHESTRATOR) ? "NoCoverageOnTests:NoCoverageOnTests:8A3B715A-6E95-4BC1-93C6-A59E9D3F5D5C:UnitTest1.cs" : "NoCoverageOnTests:MyLib.Tests/UnitTest1.cs";
    assertThat(Tests.getComponent(unitTestComponentId)).isNotNull();
    assertThat(getMeasureAsInt("NoCoverageOnTests", "lines_to_cover")).isNull();
    assertThat(getMeasureAsInt("NoCoverageOnTests", "uncovered_lines")).isNull();
  }

  @Test
  @Ignore // FIXME
  public void should_support_wildcard_patterns() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.ncover3.reportsPaths", "reports/*.nccov");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
  }

  private BuildResult analyzeCoverageTestProject(String... keyValues) throws IOException {
    Path projectDir = Tests.projectDir(temp, "CoverageTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setProperty("sonar.verbose", "true")
      .setProfile("no_rule")
      .setProperties(keyValues));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    return orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));
  }

}
