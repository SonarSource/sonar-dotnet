/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2020 SonarSource SA
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

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import java.io.IOException;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class CoverageTest {

  private final String programComponentIdCs = "CSharpVBNetCoverage:CSharpConsoleApp/Program.cs";
  private final String programComponentIdVb = "CSharpVBNetCoverage:VBNetConsoleApp/Module1.vb";

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() {
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void without_coverage_report_still_count_lines_to_cover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject();

    assertThat(buildResult.getLogs())
      .doesNotContain("Sensor VB.NET Tests Coverage Report Import")
      .doesNotContain("Coverage Report Statistics:")
      .doesNotContain("WARN: The Code Coverage report doesn't contain any coverage data for the included files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(4);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(4);
  }

  @Test
  public void ncover3() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet.ncover3.reportsPaths", "reports/ncover3.nccov");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(6);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void open_cover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet.opencover.reportsPaths", "reports/opencover.xml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(9);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void dotcover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet.dotcover.reportsPaths", "reports/dotcover.html");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void visual_studio() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void no_coverage_on_tests() throws Exception {
    BuildResult buildResult = Tests.analyzeProject(temp, "VbNoCoverageOnTests", "vbnet_no_rule", "sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 0 main files, 0 main files with coverage, 1 test files, 0 project excluded files, 0 other language files.",
      "WARN: The Code Coverage report doesn't contain any coverage data for the included files.");

    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "files")).isEqualTo(2); // Only main files are counted
    String class1VbComponentId = "VbNoCoverageOnTests:MyLib/Class1.vb";
    assertThat(Tests.getComponent(class1VbComponentId)).isNotNull();
    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "lines_to_cover")).isNull();
    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "uncovered_lines")).isNull();
  }

  @Test
  public void should_support_wildcard_patterns() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet.ncover3.reportsPaths", "reports/*.nccov");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(6);
  }

  @Test
  public void mix_vscoverage() throws IOException {
    BuildResult buildResult = analyzeCoverageMixProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml",
      "sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor C# Tests Coverage Report Import",
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 2 files, 1 main files, 1 main files with coverage, 1 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "lines_to_cover")).isEqualTo(10);
    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "uncovered_lines")).isEqualTo(4);

    assertThat(getMeasureAsInt(programComponentIdCs, "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(programComponentIdCs, "uncovered_lines")).isEqualTo(2);

    assertThat(getMeasureAsInt(programComponentIdVb, "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(programComponentIdVb, "uncovered_lines")).isEqualTo(2);
  }

  @Test
  public void mix_only_cs_vscoverage() throws IOException {
    analyzeCoverageMixProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "lines_to_cover")).isEqualTo(6);
    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "uncovered_lines")).isEqualTo(3);

    assertThat(getMeasureAsInt(programComponentIdCs, "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(programComponentIdCs, "uncovered_lines")).isEqualTo(2);

    assertThat(getMeasureAsInt(programComponentIdVb, "lines_to_cover")).isEqualTo(1);
    assertThat(getMeasureAsInt(programComponentIdVb, "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void mix_only_vbnet_vscoverage() throws IOException {
    analyzeCoverageMixProject("sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "lines_to_cover")).isEqualTo(6);
    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "uncovered_lines")).isEqualTo(3);

    assertThat(getMeasureAsInt(programComponentIdCs, "lines_to_cover")).isEqualTo(1);
    assertThat(getMeasureAsInt(programComponentIdCs, "uncovered_lines")).isEqualTo(1);

    assertThat(getMeasureAsInt(programComponentIdVb, "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(programComponentIdVb, "uncovered_lines")).isEqualTo(2);
  }

  private BuildResult analyzeCoverageTestProject(String... keyValues) throws IOException {
    return Tests.analyzeProject(temp, "VbCoverageTest", "vbnet_no_rule", keyValues);
  }

  private BuildResult analyzeCoverageMixProject(String... keyValues) throws IOException {
    return Tests.analyzeProject(temp, "CSharpVBNetCoverage", null, keyValues);
  }
}
