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
import java.nio.file.Path;
import java.util.List;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.shared.Tests.ORCHESTRATOR;
import static com.sonar.it.shared.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class VbMainCodeCsTestCodeTest {

  @TempDir
  private static Path temp;

  private static final String SONAR_RULES_PREFIX = "vbnet:";
  private static final String PROJECT = "VbMainCsTest";
  private static BuildResult buildResult;

  @BeforeAll
  public static void init() throws Exception {
    buildResult = Tests.analyzeProject(temp, PROJECT);
  }

  @Test
  public void hasIssues() {
    List<Issues.Issue> issues = TestUtils.getIssues(Tests.ORCHESTRATOR, PROJECT);
    assertThat(issues).hasSize(3)
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        SONAR_RULES_PREFIX + "S117",
        SONAR_RULES_PREFIX + "S1481",
        SONAR_RULES_PREFIX + "S6145");
  }

  @Test
  public void logsContainInfo() {
    assertThat(buildResult.getLogsLines(l -> l.contains("WARN")))
      .contains("WARN: SonarScanner for .NET detected only TEST files and no MAIN files for C# in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");

    assertThat(buildResult.getLogsLines(l -> l.contains("INFO"))).contains(
      "INFO: Found 1 MSBuild VB.NET project: 1 MAIN project.",
      "INFO: Found 1 MSBuild C# project: 1 TEST project.");
    assertThat(TestUtils.getAnalysisWarningsTask(ORCHESTRATOR, buildResult).getWarningsList()).containsExactly(
      "Multi-Language analysis is enabled. If this was not intended and you have issues such as hitting your LOC limit or analyzing unwanted files, please set \"/d:sonar.scanner.scanAll=false\" in the begin step.");
  }

  @Test
  public void metrics_are_imported_only_for_main_proj() throws Exception {
    assertThat(getMeasureAsInt("VbMainCsTest:VbMain", "files")).isEqualTo(1);
    assertThat(getMeasureAsInt("VbMainCsTest:VbMain", "lines")).isEqualTo(11);
    assertThat(getMeasureAsInt("VbMainCsTest:VbMain", "ncloc")).isEqualTo(8);

    assertThat(getMeasureAsInt("VbMainCsTest:CsTest", "files")).isNull();
    assertThat(getMeasureAsInt("VbMainCsTest:CsTest", "lines")).isNull();
    assertThat(getMeasureAsInt("VbMainCsTest:CsTest", "ncloc")).isNull();
  }
}
