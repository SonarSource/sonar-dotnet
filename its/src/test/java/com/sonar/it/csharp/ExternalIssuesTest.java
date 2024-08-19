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

import java.nio.file.Path;
import java.util.List;
import java.util.stream.Collectors;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;
import static org.sonarqube.ws.Common.RuleType;

@ExtendWith(Tests.class)
class ExternalIssuesTest {

  @TempDir
  private static Path temp;

  private static final String MAIN_PROJECT_DIR = "ExternalIssues";
  private static final String TEST_PROJECT_DIR = "ExternalIssues.TestProject.CS";
  private static final String PROGRAM_COMPONENT_ID = ":Program.cs";
  private static final String SONAR_RULES_PREFIX = "csharpsquid:";
  // note that in the UI the prefix will be 'roslyn:'
  private static final String ROSLYN_RULES_PREFIX = "external_roslyn:";

  @Test
  void external_issues_imported_by_default_as_code_smells() throws Exception {
    var projectKey = MAIN_PROJECT_DIR + "_imported";
    var componentId = projectKey + PROGRAM_COMPONENT_ID;
    Tests.analyzeProject(projectKey, temp, MAIN_PROJECT_DIR);

    assertThat(getComponent(componentId)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(componentId);

    assertThat(allIssues).hasSize(5);
    assertThat(filter(allIssues, SONAR_RULES_PREFIX, RuleType.CODE_SMELL)).hasSize(1);
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.CODE_SMELL))
      .extracting(Issues.Issue::getLine, Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        tuple(0, ROSLYN_RULES_PREFIX + "SA1633"),
        tuple(1, ROSLYN_RULES_PREFIX + "SA1200"),
        tuple(5, ROSLYN_RULES_PREFIX + "SA1400"),
        tuple(7, ROSLYN_RULES_PREFIX + "SA1400"));
  }

  @Test
  void external_issues_imported_by_default_for_test_project() throws Exception {
    var projectKey = TEST_PROJECT_DIR + "_imported";
    Tests.analyzeProject(projectKey, temp, TEST_PROJECT_DIR);

    List<Issues.Issue> allIssues = getIssues(projectKey);
    assertThat(allIssues).hasSize(2);
    assertThat(allIssues.get(0).getRule()).isEqualTo(ROSLYN_RULES_PREFIX + "NUnit1001");
    assertThat(allIssues.get(1).getRule()).isEqualTo(ROSLYN_RULES_PREFIX + "xUnit1001");
  }

  @Test
  void external_issues_are_ignored() throws Exception {
    var projectKey = MAIN_PROJECT_DIR + "_ignored";
    var componentId = projectKey + PROGRAM_COMPONENT_ID;
    Tests.analyzeProject(projectKey, temp, MAIN_PROJECT_DIR,
      "sonar.cs.roslyn.ignoreIssues", "true");

    assertThat(getComponent(componentId)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(componentId);

    assertThat(allIssues).hasSize(1);
    assertThat(filter(allIssues, SONAR_RULES_PREFIX)).hasSize(1);
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX)).isEmpty();
  }

  @Test
  void external_issues_categories_multiple_categories_mapped() throws Exception {
    var projectKey = MAIN_PROJECT_DIR + "_categories_mapped";
    var componentId = projectKey + PROGRAM_COMPONENT_ID;
    Tests.analyzeProject(projectKey, temp, MAIN_PROJECT_DIR,
      // notice that bugCategories has a list of 2 external categories
      "sonar.cs.roslyn.bugCategories", "StyleCop.CSharp.DocumentationRules,StyleCop.CSharp.MaintainabilityRules",
      "sonar.cs.roslyn.vulnerabilityCategories", "StyleCop.CSharp.OrderingRules");

    assertThat(getComponent(componentId)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(componentId);

    assertThat(allIssues).hasSize(5);
    assertThat(filter(allIssues, SONAR_RULES_PREFIX, RuleType.CODE_SMELL)).hasSize(1);

    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.CODE_SMELL)).isEmpty();
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.BUG))
      .extracting(Issues.Issue::getLine, Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        tuple(0, ROSLYN_RULES_PREFIX + "SA1633"),
        tuple(5, ROSLYN_RULES_PREFIX + "SA1400"),
        tuple(7, ROSLYN_RULES_PREFIX + "SA1400"));
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.VULNERABILITY))
      .extracting(Issues.Issue::getLine, Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        tuple(1, ROSLYN_RULES_PREFIX + "SA1200"));
  }

  @Test
  void external_issues_all_three_properties() throws Exception {
    var projectKey = MAIN_PROJECT_DIR + "_three_properties";
    var componentId = projectKey + PROGRAM_COMPONENT_ID;
    Tests.analyzeProject(projectKey, temp, MAIN_PROJECT_DIR,
      "sonar.cs.roslyn.codeSmellCategories", "StyleCop.CSharp.DocumentationRules",
      "sonar.cs.roslyn.bugCategories", "StyleCop.CSharp.MaintainabilityRules",
      "sonar.cs.roslyn.vulnerabilityCategories", "StyleCop.CSharp.OrderingRules");

    assertThat(getComponent(componentId)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(componentId);

    assertThat(allIssues).hasSize(5);
    assertThat(filter(allIssues, SONAR_RULES_PREFIX, RuleType.CODE_SMELL)).hasSize(1);

    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.CODE_SMELL))
      .extracting(Issues.Issue::getLine, Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        tuple(0, ROSLYN_RULES_PREFIX + "SA1633"));
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.BUG))
      .extracting(Issues.Issue::getLine, Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        tuple(5, ROSLYN_RULES_PREFIX + "SA1400"),
        tuple(7, ROSLYN_RULES_PREFIX + "SA1400"));
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.VULNERABILITY))
      .extracting(Issues.Issue::getLine, Issues.Issue::getRule)
      .containsExactlyInAnyOrder(
        tuple(1, ROSLYN_RULES_PREFIX + "SA1200"));
  }

  private List<Issues.Issue> filter(List<Issues.Issue> issues, String ruleIdPrefix) {
    return issues
      .stream()
      .filter(x -> x.getRule().startsWith(ruleIdPrefix))
      .collect(Collectors.toList());
  }

  private List<Issues.Issue> filter(List<Issues.Issue> issues, String ruleIdPrefix, RuleType ruleType) {
    return issues
      .stream()
      .filter(x -> x.getRule().startsWith(ruleIdPrefix) && x.getType() == ruleType)
      .collect(Collectors.toList());
  }
}
