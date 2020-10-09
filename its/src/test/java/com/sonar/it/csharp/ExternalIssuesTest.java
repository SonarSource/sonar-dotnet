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
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import java.util.List;
import java.util.stream.Collectors;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarqube.ws.Common.RuleType;

public class ExternalIssuesTest {

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String PROJECT = "ExternalIssues";
  private static final String PROGRAM_COMPONENT_ID = "ExternalIssues:Program.cs";
  private static final String SONAR_RULES_PREFIX = "csharpsquid:";
  // note that in the UI the prefix will be 'roslyn:'
  private static final String ROSLYN_RULES_PREFIX = "external_roslyn:";


  @Before
  public void init() {
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void external_issues_imported_by_default_as_code_smells() throws Exception {
    Tests.analyzeProject(temp, PROJECT, null);

    assertThat(getComponent(PROGRAM_COMPONENT_ID)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(PROGRAM_COMPONENT_ID);

    assertThat(allIssues).hasSize(5);
    assertThat(filter(allIssues, SONAR_RULES_PREFIX, RuleType.CODE_SMELL)).hasSize(1);
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.CODE_SMELL)).hasSize(4);
  }

  @Test
  public void external_issues_are_ignored() throws Exception {
    Tests.analyzeProject(temp, PROJECT, null,
      "sonar.cs.roslyn.ignoreIssues", "true");

    assertThat(getComponent(PROGRAM_COMPONENT_ID)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(PROGRAM_COMPONENT_ID);

    assertThat(allIssues).hasSize(1); // our rule
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX)).isEmpty();
  }

  @Test
  public void external_issues_categories_mapping() throws Exception {
    Tests.analyzeProject(temp, PROJECT, null,
      "sonar.cs.roslyn.bugCategories", "StyleCop.CSharp.DocumentationRules,StyleCop.CSharp.MaintainabilityRules",
      "sonar.cs.roslyn.vulnerabilityCategories", "StyleCop.CSharp.OrderingRules");

    assertThat(getComponent(PROGRAM_COMPONENT_ID)).isNotNull();
    List<Issues.Issue> allIssues = getIssues(PROGRAM_COMPONENT_ID);

    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.CODE_SMELL)).isEmpty();
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.VULNERABILITY)).hasSize(1);
    assertThat(filter(allIssues, ROSLYN_RULES_PREFIX, RuleType.BUG)).hasSize(3);
    assertThat(filter(allIssues, SONAR_RULES_PREFIX, RuleType.CODE_SMELL)).hasSize(1);
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
