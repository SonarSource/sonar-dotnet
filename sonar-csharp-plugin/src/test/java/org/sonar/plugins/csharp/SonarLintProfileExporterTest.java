/*
 * SonarC#
 * Copyright (C) 2014-2017 SonarSource SA
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
package org.sonar.plugins.csharp;

import com.google.common.collect.ImmutableSet;
import java.io.StringWriter;
import java.util.Collections;
import java.util.Set;
import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;
import org.sonar.api.rules.RulePriority;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class SonarLintProfileExporterTest {

  @Test
  public void test() {
    // S1000 has parameters and is enabled
    Rule ruleS1000 = mock(Rule.class);
    when(ruleS1000.getKey()).thenReturn("S1000");
    RuleParam ruleParam = mock(RuleParam.class);
    when(ruleS1000.getParams()).thenReturn(Collections.singletonList(ruleParam));
    when(ruleS1000.getTemplate()).thenReturn(null);
    when(ruleS1000.getSeverity()).thenReturn(RulePriority.MAJOR);
    org.sonar.api.rules.ActiveRule activeRuleS1000 = mock(ActiveRule.class);
    when(activeRuleS1000.getRule()).thenReturn(ruleS1000);
    when(activeRuleS1000.getSeverity()).thenReturn(RulePriority.BLOCKER);

    // S1001 is a SonarLint rule and disabled -> should be disabled in exported rule set
    Rule ruleS1001 = mock(Rule.class);
    when(ruleS1001.getKey()).thenReturn("S1001");
    when(ruleS1001.getParams()).thenReturn(Collections.emptyList());
    when(ruleS1001.getTemplate()).thenReturn(null);
    when(ruleS1001.getSeverity()).thenReturn(RulePriority.MAJOR);

    Set<String> allRules = ImmutableSet.of(
      ruleS1000.getKey(),
      ruleS1001.getKey());
    CSharpSonarRulesDefinition csharpRulesDefinition = mock(CSharpSonarRulesDefinition.class);
    when(csharpRulesDefinition.allRuleKeys()).thenReturn(allRules);

    SonarLintProfileExporter exporter = new SonarLintProfileExporter(csharpRulesDefinition);
    assertThat(exporter.getKey()).isEqualTo("sonarlint-vs-cs");
    assertThat(exporter.getName()).isEqualTo("SonarLint for Visual Studio Rule Set");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");

    StringWriter writer = new StringWriter();
    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository(CSharpSonarRulesDefinition.REPOSITORY_KEY)).thenReturn(Collections.singletonList(activeRuleS1000));
    exporter.exportProfile(rulesProfile, writer);
    assertThat(writer.toString()).isEqualTo(
      "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        "<RuleSet Name=\"Rules for SonarLint\" Description=\"This rule set was automatically generated from SonarQube.\" ToolsVersion=\"14.0\">\r\n" +
        "  <Rules AnalyzerId=\"SonarAnalyzer.CSharp\" RuleNamespace=\"SonarAnalyzer.CSharp\">\r\n" +
        "    <Rule Id=\"S1000\" Action=\"Warning\" />\r\n" +
        "    <Rule Id=\"S1001\" Action=\"None\" />\r\n" +
        "  </Rules>\r\n" +
        "</RuleSet>\r\n");
  }

}
