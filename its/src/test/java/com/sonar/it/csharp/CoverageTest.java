/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2024 SonarSource SA
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

import com.sonar.orchestrator.build.BuildResult;
import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Measures.Measure;

import static com.sonar.it.csharp.Tests.getMeasure;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class CoverageTest {

  @TempDir
  private static Path temp;

  @Test
  void should_not_import_coverage_without_report() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject();

    assertThat(buildResult.getLogs())
      .doesNotContain("Sensor C# Tests Coverage Report Import")
      .doesNotContain("Coverage Report Statistics:");

    Measure linesToCover = getMeasure("CoverageTest", "lines_to_cover");
    Measure uncoveredLines = getMeasure("CoverageTest", "uncovered_lines");

    assertThat(linesToCover.getValue()).isEqualTo("2");
    assertThat(uncoveredLines.getValue()).isEqualTo("2");
  }

  @Test
  void ncover3() throws Exception {
    String reportPath = temp.toAbsolutePath() + File.separator + "CoverageTest" + File.separator + "reports" + File.separator + "ncover3.nccov";
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.ncover3.reportsPaths", reportPath);

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");
    assertLineCoverageMetrics("CoverageTest", 2, 1);
  }

  @Test
  void open_cover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.opencover.reportsPaths", "reports/opencover.xml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertLineCoverageMetrics("CoverageTest", 2, 0);
  }

  @Test
  void open_cover_on_MultipleProjects() throws Exception {
    BuildResult buildResult = analyzeMultipleProjectsTestProject("sonar.cs.opencover.reportsPaths", "opencover.xml");
    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 5 files, 3 main files, 3 main files with coverage, 2 test files, 0 project excluded files, 0 other language files");

    assertCoverageMetrics("CoverageTest.MultipleProjects", 25, 3, 12, 5);
    assertCoverageMetrics("CoverageTest.MultipleProjects:FirstProject/FirstClass.cs", 10, 0, 2, 1);
    assertLineCoverageMetrics("CoverageTest.MultipleProjects:FirstProject/SecondClass.cs", 4, 0);
    assertCoverageMetrics("CoverageTest.MultipleProjects:SecondProject/FirstClass.cs", 11, 3, 10, 4);
  }

  @Test
  void open_cover_on_MultipleFrameworks() throws Exception {
    BuildResult buildResult = analyzeMultipleFrameworksTestProject("sonar.cs.opencover.reportsPaths", "coverage.*.xml");
    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 4 files, 4 main files, 4 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertCoverageMetrics("CoverageTest.MultipleFrameworks", 32, 4, 8, 1);
    assertCoverageMetrics("CoverageTest.MultipleFrameworks:ClassLibrary/MatchingBranchpoints_FullyCovered.cs", 8, 0, 2, 0);
    assertCoverageMetrics("CoverageTest.MultipleFrameworks:ClassLibrary/MatchingBranchpoints_PartiallyCovered.cs", 8, 2, 2, 1);
    assertCoverageMetrics("CoverageTest.MultipleFrameworks:ClassLibrary/NotMatchingBranchpoints_FullyCovered.cs", 8, 0, 2, 0);
    // We are over-reporting covered conditions in NotMatchingBranchpoints_PartiallyCovered.cs. This is an expected trade-off to make coverage aggregation more consistent.
    assertCoverageMetrics("CoverageTest.MultipleFrameworks:ClassLibrary/NotMatchingBranchpoints_PartiallyCovered.cs", 8, 2, 2, 0);
  }

  @Test
  void open_cover_on_MultipleProjects_with_UnixWildcardPattern() throws Exception {
    BuildResult buildResult = analyzeMultipleProjectsTestProject("sonar.cs.opencover.reportsPaths", "/*/opencover.xml");

    // The original opencover is moved to a subfolder and parts of it are removed and that is how we know the correct one is matched.
    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 3 files, 1 main files, 1 main files with coverage, 2 test files, 0 project excluded files, 0 other language files");

    assertCoverageMetrics("CoverageTest.MultipleProjects", 19, 11, 10, 4);
  }

  @Test
  void open_cover_with_deterministic_source_paths() throws Exception {
    BuildResult buildResult = Tests.analyzeProject(temp, "CoverageWithDeterministicSourcePaths", "sonar.cs.opencover.reportsPaths", "opencover.xml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    Measure linesToCover = getMeasure("CoverageWithDeterministicSourcePaths", "lines_to_cover");
    Measure uncoveredLines = getMeasure("CoverageWithDeterministicSourcePaths", "uncovered_lines");

    assertThat(linesToCover.getValue()).isEqualTo("6");
    assertThat(uncoveredLines.getValue()).isEqualTo("3");
    assertCoverageMetrics("CoverageWithDeterministicSourcePaths", 6, 3, 2, 1);
  }

  @Test
  void dotcover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.dotcover.reportsPaths", "reports/dotcover.html");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertLineCoverageMetrics("CoverageTest", 2, 1);
  }

  @Test
  void visual_studio() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertLineCoverageMetrics("CoverageTest", 2, 1);
  }

  @Test
  void visual_studio_on_MultipleProjects() throws Exception {
    BuildResult buildResult = analyzeMultipleProjectsTestProject("sonar.cs.vscoveragexml.reportsPaths", "VisualStudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 5 files, 3 main files, 3 main files with coverage, 2 test files, 0 project excluded files, 0 other language files.");

    assertLineCoverageMetrics("CoverageTest.MultipleProjects", 22, 2);
    assertLineCoverageMetrics("CoverageTest.MultipleProjects:FirstProject/FirstClass.cs", 10, 0);
    assertLineCoverageMetrics("CoverageTest.MultipleProjects:FirstProject/SecondClass.cs", 4, 0);
    assertLineCoverageMetrics("CoverageTest.MultipleProjects:SecondProject/FirstClass.cs", 8, 2);
  }

  @Test
  void visual_studio_with_deterministic_source_paths() throws Exception {
    BuildResult buildResult = Tests.analyzeProject(temp, "CoverageWithDeterministicSourcePaths", "sonar.cs.vscoveragexml.reportsPaths", "VisualStudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    Measure linesToCover = getMeasure("CoverageWithDeterministicSourcePaths", "lines_to_cover");
    Measure uncoveredLines = getMeasure("CoverageWithDeterministicSourcePaths", "uncovered_lines");

    assertThat(linesToCover.getValue()).isEqualTo("6");
    assertThat(uncoveredLines.getValue()).isEqualTo("3");
  }

  @Test
  void no_coverage_on_tests() throws Exception {
    BuildResult buildResult = Tests.analyzeProject(temp, "NoCoverageOnTests", "sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 0 main files, 0 main files with coverage, 1 test files, 0 project excluded files, 0 other language files.",
      "WARN: The Code Coverage report doesn't contain any coverage data for the included files.");

    assertThat(getMeasureAsInt("NoCoverageOnTests", "files")).isEqualTo(1); // Only main files are counted: Class1.cs

    assertThat(Tests.getComponent("NoCoverageOnTests:MyLib.Tests/UnitTest1.cs")).isNotNull();
    assertNoCoverageMetrics("NoCoverageOnTests");
  }

  @Test
  void should_support_wildcard_patterns() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.ncover3.reportsPaths", "reports/*.nccov");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
  }

  private BuildResult analyzeCoverageTestProject(String... keyValues) throws IOException {
    return Tests.analyzeProject(temp, "CoverageTest", keyValues);
  }

  private BuildResult analyzeMultipleProjectsTestProject(String coverageProperty, String coverageFileName) throws IOException {
    return Tests.analyzeProject(temp, "CoverageTest.MultipleProjects", coverageProperty, coverageFileName);
  }

  private BuildResult analyzeMultipleFrameworksTestProject(String coverageProperty, String coverageFileNames) throws IOException {
    return Tests.analyzeProject(temp, "CoverageTest.MultipleFrameworks", coverageProperty, coverageFileNames);
  }

  private void assertCoverageMetrics(String componentKey, int linesToCover, int uncoveredLines, int conditionsToCover, int uncoveredConditions) {
    assertThat(getMeasureAsInt(componentKey, "lines_to_cover")).isEqualTo(linesToCover);
    assertThat(getMeasureAsInt(componentKey, "uncovered_lines")).isEqualTo(uncoveredLines);
    assertThat(getMeasureAsInt(componentKey, "conditions_to_cover")).isEqualTo(conditionsToCover);
    assertThat(getMeasureAsInt(componentKey, "uncovered_conditions")).isEqualTo(uncoveredConditions);
  }

  private void assertLineCoverageMetrics(String componentKey, int linesToCover, int uncoveredLines) {
    assertThat(getMeasureAsInt(componentKey, "lines_to_cover")).isEqualTo(linesToCover);
    assertThat(getMeasureAsInt(componentKey, "uncovered_lines")).isEqualTo(uncoveredLines);
    assertThat(getMeasureAsInt(componentKey, "conditions_to_cover")).isNull();
    assertThat(getMeasureAsInt(componentKey, "uncovered_conditions")).isNull();
  }

  private void assertNoCoverageMetrics(String componentKey) {
    assertThat(getMeasureAsInt(componentKey, "lines_to_cover")).isNull();
    assertThat(getMeasureAsInt(componentKey, "uncovered_lines")).isNull();
    assertThat(getMeasureAsInt(componentKey, "conditions_to_cover")).isNull();
    assertThat(getMeasureAsInt(componentKey, "uncovered_conditions")).isNull();
  }
}
