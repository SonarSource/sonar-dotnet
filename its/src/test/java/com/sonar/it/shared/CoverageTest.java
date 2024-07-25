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
package com.sonar.it.shared;

import com.sonar.orchestrator.build.BuildResult;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import static com.sonar.it.shared.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class CoverageTest {

  private final String programComponentIdCs = "CSharpVBNetCoverage:CSharpConsoleApp/Program.cs";
  private final String programComponentIdVb = "CSharpVBNetCoverage:VBNetConsoleApp/Module1.vb";

  @TempDir
  private static Path temp;

  @Test
  public void mix_vscoverage() throws IOException {
    BuildResult buildResult = analyzeCoverageMixProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml",
      "sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains(
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

  private BuildResult analyzeCoverageMixProject(String... keyValues) throws IOException {
    return Tests.analyzeProject(temp, "CSharpVBNetCoverage", keyValues);
  }
}
