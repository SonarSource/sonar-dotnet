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
package com.sonar.it.vbnet;

import com.sonar.orchestrator.build.BuildResult;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class CoverageTest {

  @TempDir
  private static Path temp;

  @Test
  void without_coverage_report_still_count_lines_to_cover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject();

    assertThat(buildResult.getLogs())
      .doesNotContain("Sensor VB.NET Tests Coverage Report Import")
      .doesNotContain("Coverage Report Statistics:")
      .doesNotContain("WARN: The Code Coverage report doesn't contain any coverage data for the included files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(4);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(4);
  }

  @ParameterizedTest
  @CsvSource(value = {"ncover3:nccov:6:1", "opencover:xml:9:1", "dotcover:html:5:1", "vscoveragexml:visualstudio.coveragexml:5:1"}, delimiter = ':')
  void coverage(String testFramework,  String format, int linesToCover, int uncoveredLines) throws Exception {
    String reportFile = testFramework.equals("vscoveragexml") ? format : testFramework + "." + format;
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet." + testFramework + ".reportsPaths", "reports/" + reportFile);

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(linesToCover);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(uncoveredLines);
  }

  @Test
  void no_coverage_on_tests() throws Exception {
    BuildResult buildResult = Tests.analyzeProject(temp, "VbNoCoverageOnTests", "sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 0 main files, 0 main files with coverage, 1 test files, 0 project excluded files, 0 other language files.",
      "WARN: The Code Coverage report doesn't contain any coverage data for the included files.");

    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "files")).isEqualTo(1); // Only main files are counted: Class1.vb
    String class1VbComponentId = "VbNoCoverageOnTests:MyLib/Class1.vb";
    assertThat(Tests.getComponent(class1VbComponentId)).isNotNull();
    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "lines_to_cover")).isNull();
    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "uncovered_lines")).isNull();
  }

  @Test
  void should_support_wildcard_patterns() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.vbnet.ncover3.reportsPaths", "reports/*.nccov");

    assertThat(buildResult.getLogs()).contains(
      "Sensor VB.NET Tests Coverage Report Import",
      "Coverage Report Statistics: 1 files, 1 main files, 1 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(6);
  }

  private BuildResult analyzeCoverageTestProject(String... keyValues) throws IOException {
    return Tests.analyzeProject(temp, "VbCoverageTest", keyValues);
  }
}
