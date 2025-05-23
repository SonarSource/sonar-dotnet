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
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Collectors;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class TestProjectTest {

  @TempDir
  private static Path temp;

  private static final String CSHARP_ONLY_TEST_PROJECT = "TestOnlyProject";
  // the below is explicitly marked as test with <SonarQubeTestProject> MSBuild property
  private static final String EXPLICITLY_MARKED_AS_TEST = "HtmlCSharpExplicitlyMarkedAsTest";

  private static BuildResult buildResult;

  @BeforeAll
  public static void init() throws Exception {
    buildResult = Tests.analyzeProject(temp, CSHARP_ONLY_TEST_PROJECT);
  }

  @Test
  void projectIsAnalyzed() {
    assertThat(getComponent(CSHARP_ONLY_TEST_PROJECT).getName()).isEqualTo("TestOnlyProject");

    assertThat(getComponent("TestOnlyProject:UnitTest1.cs").getName()).isEqualTo("UnitTest1.cs");
  }

  @Test
  void logsContainInfoAndWarning() {
    assertThat(buildResult.getLogs()).contains(
      "SonarScanner for .NET detected only TEST files and no MAIN files for C# in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects",
      "Found 1 MSBuild C# project: 1 TEST project.");
    TestUtils.verifyGuiTestOnlyProjectAnalysisWarning(ORCHESTRATOR, buildResult, "C#", "Multi-Language analysis is enabled. If this was not intended and you have issues such as hitting your LOC limit or analyzing unwanted files, please set \"/d:sonar.scanner.scanAll=false\" in the begin step.");
  }

  @Test
  void with_csharp_only_test_should_not_populate_metrics() {
    assertThat(Tests.getComponent("TestOnlyProject:UnitTest1.cs")).isNotNull();
    assertThat(getMeasureAsInt(CSHARP_ONLY_TEST_PROJECT, "files")).isNull();
    assertThat(getMeasureAsInt(CSHARP_ONLY_TEST_PROJECT, "lines")).isNull();
    assertThat(getMeasureAsInt(CSHARP_ONLY_TEST_PROJECT, "ncloc")).isNull();

    assertThat(buildResult.getLogsLines(l -> l.contains("INFO"))).contains("INFO: Found 1 MSBuild C# project: 1 TEST project.");
  }

  @Test
  void issuesAreImportedForTestProject() {
    List<Issues.Issue> barIssues = getIssues("TestOnlyProject:UnitTest1.cs")
      .stream()
      .filter(x -> x.getRule().startsWith("csharpsquid:"))
      .collect(Collectors.toList());

    assertThat(barIssues).hasSize(1); // Rule S1125 is not triggered for now, because it has scope MAIN & TEST

    assertThat(barIssues)
      .filteredOn(e -> e.getRule().equalsIgnoreCase("csharpsquid:S1607"))
      .hasOnlyOneElementSatisfying(e -> assertThat(e.getLine()).isEqualTo(15));
  }

  @Test
  void with_html_and_csharp_code_explicitly_marked_as_test_should_not_populate_metrics() throws Exception {
    BuildResult buildResult = Tests.analyzeProject(temp, EXPLICITLY_MARKED_AS_TEST);

    assertThat(Tests.getComponent("HtmlCSharpExplicitlyMarkedAsTest:Foo.cs")).isNotNull();
    assertThat(getMeasureAsInt(EXPLICITLY_MARKED_AS_TEST, "files")).isNull();
    assertThat(getMeasureAsInt(EXPLICITLY_MARKED_AS_TEST, "lines")).isNull();
    assertThat(getMeasureAsInt(EXPLICITLY_MARKED_AS_TEST, "ncloc")).isNull();

    assertThat(buildResult.getLogsLines(l -> l.contains("WARN")))
      .contains("WARN: SonarScanner for .NET detected only TEST files and no MAIN files for C# in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");
    assertThat(buildResult.getLogsLines(l -> l.contains("INFO"))).contains("INFO: Found 1 MSBuild C# project: 1 TEST project.");
    TestUtils.verifyGuiTestOnlyProjectAnalysisWarning(ORCHESTRATOR, buildResult, "C#", "Multi-Language analysis is enabled. If this was not intended and you have issues such as hitting your LOC limit or analyzing unwanted files, please set \"/d:sonar.scanner.scanAll=false\" in the begin step.");
  }
}
