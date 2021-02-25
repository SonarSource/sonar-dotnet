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
package com.sonar.it.vbnet;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;
import java.util.List;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Ce;
import org.sonarqube.ws.Issues;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

public class VbMainCodeCsTestCodeTest {

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @ClassRule
  public static final TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String SONAR_RULES_PREFIX = "vbnet:";
  private static final String PROJECT = "VbMainCsTest";
  private static BuildResult buildResult;

  @BeforeClass
  public static void init() throws Exception {
    TestUtils.reset(orchestrator);
    buildResult = Tests.analyzeProject(temp, PROJECT, "vbnet_class_name");
  }

  @Test
  public void hasIssues() {
    List<Issues.Issue> issues = TestUtils.getIssues(Tests.ORCHESTRATOR, PROJECT);
    assertThat(issues).hasSize(3)
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        SONAR_RULES_PREFIX + "S117",
        SONAR_RULES_PREFIX + "S1481",
        SONAR_RULES_PREFIX + "S6145"
      );
  }

  @Test
  public void logsContainInfo() {
    assertThat(buildResult.getLogsLines(l -> l.contains("WARN")))
      .contains("WARN: This C# sensor will be skipped, because the current solution contains only TEST files and no MAIN files. " +
        "Your SonarQube/SonarCloud project will not have results for C# files. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");
    assertThat(buildResult.getLogsLines(l -> l.contains("INFO"))).contains(
      "INFO: Found 1 MSBuild VB.NET project: 1 MAIN project.",
      "INFO: Found 1 MSBuild C# project: 1 TEST project."
    );
    verifyNoGuiWarnings(buildResult);
  }

  // Verifies the analysis warning is raised inside SQ
  private void verifyNoGuiWarnings(BuildResult buildResult) {
    Ce.Task task = TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).isEmpty();
  }
}
