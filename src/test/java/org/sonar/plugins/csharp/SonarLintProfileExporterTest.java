/*
 * SonarQube C# Plugin
 * Copyright (C) 2014 SonarSource
 * sonarqube@googlegroups.com
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableSet;
import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;
import org.sonar.api.rules.RulePriority;

import java.io.StringWriter;
import java.util.Set;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class SonarLintProfileExporterTest {

  @Test
  public void test() {
    // S1000 has parameters and is enabled -> should not be in exported rule set
    Rule ruleS1000 = mock(Rule.class);
    when(ruleS1000.getKey()).thenReturn("S1000");
    RuleParam ruleParam = mock(RuleParam.class);
    when(ruleS1000.getParams()).thenReturn(ImmutableList.of(ruleParam));
    when(ruleS1000.getTemplate()).thenReturn(null);
    when(ruleS1000.getSeverity()).thenReturn(RulePriority.MAJOR);
    org.sonar.api.rules.ActiveRule activeRuleS1000 = mock(ActiveRule.class);
    when(activeRuleS1000.getRule()).thenReturn(ruleS1000);
    when(activeRuleS1000.getSeverity()).thenReturn(RulePriority.BLOCKER);

    // S1001 is a template rule and is enabled -> should not be in exported rule set
    Rule ruleS1001 = mock(Rule.class);
    when(ruleS1001.getKey()).thenReturn("S1001");
    when(ruleS1001.getParams()).thenReturn(ImmutableList.<RuleParam>of());
    Rule baseTemplateRule = mock(Rule.class);
    when(ruleS1001.getTemplate()).thenReturn(baseTemplateRule);
    when(ruleS1001.getSeverity()).thenReturn(RulePriority.MAJOR);
    org.sonar.api.rules.ActiveRule activeRuleS1001 = mock(ActiveRule.class);
    when(activeRuleS1001.getRule()).thenReturn(ruleS1001);
    when(activeRuleS1001.getSeverity()).thenReturn(RulePriority.BLOCKER);

    // S1002 is a SonarLint rule and disabled -> should be disabled in exported rule set
    Rule ruleS1002 = mock(Rule.class);
    when(ruleS1002.getKey()).thenReturn("S1002");
    when(ruleS1002.getParams()).thenReturn(ImmutableList.<RuleParam>of());
    when(ruleS1002.getTemplate()).thenReturn(null);
    when(ruleS1002.getSeverity()).thenReturn(RulePriority.MAJOR);

    // S1003 is a SonarLint rule and enabled at default severity -> should not be in exported rule set
    Rule ruleS1003 = mock(Rule.class);
    when(ruleS1003.getKey()).thenReturn("S1003");
    when(ruleS1003.getParams()).thenReturn(ImmutableList.<RuleParam>of());
    when(ruleS1003.getTemplate()).thenReturn(null);
    when(ruleS1003.getSeverity()).thenReturn(RulePriority.MAJOR);
    org.sonar.api.rules.ActiveRule activeRuleS1003 = mock(ActiveRule.class);
    when(activeRuleS1003.getRule()).thenReturn(ruleS1003);
    when(activeRuleS1003.getSeverity()).thenReturn(RulePriority.MAJOR);

    // S1004 is a SonarLint rule and enabled at different severity -> should be in exported rule set
    Rule ruleS1004 = mock(Rule.class);
    when(ruleS1004.getKey()).thenReturn("S1004");
    when(ruleS1004.getParams()).thenReturn(ImmutableList.<RuleParam>of());
    when(ruleS1004.getTemplate()).thenReturn(null);
    when(ruleS1004.getSeverity()).thenReturn(RulePriority.MAJOR);
    org.sonar.api.rules.ActiveRule activeRuleS1004 = mock(ActiveRule.class);
    when(activeRuleS1004.getRule()).thenReturn(ruleS1004);
    when(activeRuleS1004.getSeverity()).thenReturn(RulePriority.BLOCKER);

    Set<String> allRules = ImmutableSet.of(
      ruleS1000.getKey(),
      ruleS1001.getKey(),
      ruleS1002.getKey(),
      ruleS1003.getKey(),
      ruleS1004.getKey());
    CSharpSonarRulesDefinition csharpRulesDefinition = mock(CSharpSonarRulesDefinition.class);
    when(csharpRulesDefinition.parameterlessRuleKeys()).thenReturn(allRules);

    SonarLintProfileExporter exporter = new SonarLintProfileExporter(csharpRulesDefinition);
    assertThat(exporter.getKey()).isEqualTo("sonarlint-vs-cs");
    assertThat(exporter.getName()).isEqualTo("SonarLint for Visual Studio Rule Set");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");

    StringWriter writer = new StringWriter();
    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository(CSharpPlugin.REPOSITORY_KEY)).thenReturn(
      ImmutableList.of(
        activeRuleS1000,
        activeRuleS1001,
        activeRuleS1003,
        activeRuleS1004));
    exporter.exportProfile(rulesProfile, writer);
    assertThat(writer.toString()).isEqualTo(
      "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        "<RuleSet Name=\"Rules for SonarLint\" Description=\"This rule set was automatically generated from SonarQube.\" ToolsVersion=\"14.0\">\r\n" +
        "  <Rules AnalyzerId=\"SonarLint\" RuleNamespace=\"SonarLint\">\r\n" +
        "    <Rule Id=\"S1004\" Action=\"Warning\" />\r\n" +
        "    <Rule Id=\"S1002\" Action=\"None\" />\r\n" +
        "  </Rules>\r\n" +
        "</RuleSet>\r\n");
  }

}
