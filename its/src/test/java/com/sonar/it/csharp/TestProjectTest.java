/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2021 SonarSource SA
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
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Ce;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static org.assertj.core.api.Assertions.assertThat;

public class TestProjectTest {
  @ClassRule
  public static final TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String PROJECT = "TestOnlyProject";
  private static BuildResult buildResult;

  @BeforeClass
  public static void init() throws Exception {
    TestUtils.reset(ORCHESTRATOR);
    buildResult = Tests.analyzeProject(temp, PROJECT, null);
  }

  @Test
  public void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo("TestOnlyProject");

    assertThat(getComponent("TestOnlyProject:UnitTest1.cs").getName()).isEqualTo("UnitTest1.cs");
  }

  @Test
  public void logsContainInfoAndWarning() {
    assertThat(buildResult.getLogs()).contains(
      "This C# sensor will be skipped, because the current solution contains only TEST files and no MAIN files. " +
        "Your SonarQube/SonarCloud project will not have results for C# files. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects",
      "Found 1 MSBuild C# project: 1 TEST project."
    );
    verifyGuiAnalysisWarning(buildResult);
  }

  // Verifies the analysis warning is raised inside SQ
  private void verifyGuiAnalysisWarning(BuildResult buildResult) {
    Ce.Task task = TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).containsExactly("Your project contains only TEST code for language C# and no MAIN code for any language, so no results have been imported. " +
      "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");
  }
}
