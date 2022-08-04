/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2022 SonarSource SA
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
import com.sonar.it.shared.Tests;
import com.sonar.orchestrator.build.BuildResult;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Ce;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

import static com.sonar.it.shared.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

public class AnalysisWarningsTest {
  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() {
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void analysisWarningsImport() throws IOException {
    Path projectDir = Tests.projectDir(temp, "Empty");

    ORCHESTRATOR.executeBuild(TestUtils.createBeginStep("Empty", projectDir));
    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Restore,Rebuild");

    Path target = projectDir.resolve(".sonarqube\\out\\AnalysisWarnings.AutoScan.json");
    Files.createDirectories(target.getParent());
    Files.copy(projectDir.resolve("AnalysisWarnings.AutoScan.json"), target);
    BuildResult buildResult = ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));

    Ce.Task task = TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).containsExactly("First message", "Second message", "BROKEN IT");
  }
}
