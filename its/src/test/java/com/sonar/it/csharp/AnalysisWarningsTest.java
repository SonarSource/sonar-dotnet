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

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Ce;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class AnalysisWarningsTest {

  @TempDir
  private static Path temp;

  @Test
  void analysisWarningsImport() throws IOException {
    Path projectDir = TestUtils.projectDir(temp, "Empty");

    ORCHESTRATOR.executeBuild(TestUtils.createBeginStep("Empty", projectDir));
    TestUtils.runBuild(projectDir);

    Path target = projectDir.resolve(".sonarqube\\out\\AnalysisWarnings.AutoScan.json");
    Files.createDirectories(target.getParent());
    Files.copy(projectDir.resolve("AnalysisWarnings.AutoScan.json"), target);
    BuildResult buildResult = ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));

    Ce.Task task = TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).containsExactly(
      "First message",
      "Second message",
      "Multi-Language analysis is enabled. If this was not intended, please set \"/d:sonar.scanner.scanAll=false\" in the begin step.");
  }

  @Test
  void analysisWarnings_MSBuild14() throws IOException {
    BuildResult buildResult = Tests.analyzeProject(temp, "Roslyn.1.3.2");
    Ce.Task task = TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).containsExactly(
      "The analysis using MsBuild 14 is no longer supported and the analysis with MsBuild 15 is deprecated. Please update your pipeline to MsBuild 16 or higher.",
      "Multi-Language analysis is enabled. If this was not intended, please set \"/d:sonar.scanner.scanAll=false\" in the begin step.");
  }

  @Test
  void analysisWarnings_MSBuild15() throws IOException {
    BuildResult buildResult = Tests.analyzeProject(temp, "Roslyn.2.4.0");
    Ce.Task task = TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).containsExactly(
      "The analysis using MsBuild 15 is deprecated. Please update your pipeline to MsBuild 16 or higher.",
      "Multi-Language analysis is enabled. If this was not intended, please set \"/d:sonar.scanner.scanAll=false\" in the begin step.");
  }
}
