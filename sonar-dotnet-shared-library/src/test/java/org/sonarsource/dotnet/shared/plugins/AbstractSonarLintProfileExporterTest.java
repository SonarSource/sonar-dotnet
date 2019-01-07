/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.IOException;
import java.io.StringWriter;
import java.io.Writer;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import org.junit.Before;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.Mock;
import org.mockito.Mockito;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.doThrow;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class AbstractSonarLintProfileExporterTest {
  @org.junit.Rule
  public ExpectedException exception = ExpectedException.none();

  private AbstractSonarLintProfileExporter exporter;
  private RulesProfile rulesProfile;

  @Before
  public void setUp() {
    // S1000 has parameters and is enabled
    Rule ruleS1000 = mock(Rule.class);
    when(ruleS1000.getKey()).thenReturn("S1000");
    ActiveRule activeRuleS1000 = mock(ActiveRule.class);
    when(activeRuleS1000.getRule()).thenReturn(ruleS1000);

    // S1001 is a SonarLint rule and disabled -> should be disabled in exported rule set
    Rule ruleS1001 = mock(Rule.class);
    when(ruleS1001.getKey()).thenReturn("S1001");

    rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository("csharpsquid")).thenReturn(Collections.singletonList(activeRuleS1000));

    List<Rule> allRules = new ArrayList<>();
    allRules.add(ruleS1000);
    allRules.add(ruleS1001);
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.findAll(Mockito.any(RuleQuery.class))).thenReturn(allRules);
    exporter = new AbstractSonarLintProfileExporter("sonarlint-vs-cs", "SonarLint for Visual Studio Rule Set", "cs", "SonarAnalyzer.CSharp", "csharpsquid", ruleFinder) {
    };
  }

  @Test
  public void fail_if_cant_write_file() throws IOException {
    Writer writer = mock(Writer.class);
    doThrow(new IOException()).when(writer).write(Mockito.anyString());

    exception.expect(IllegalStateException.class);
    exporter.exportProfile(rulesProfile, writer);
  }

  @Test
  public void exporter_should_write_file() {
    StringWriter writer = new StringWriter();
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

  @Test
  public void exporter_should_describe_itself() {
    assertThat(exporter.getKey()).isEqualTo("sonarlint-vs-cs");
    assertThat(exporter.getName()).isEqualTo("SonarLint for Visual Studio Rule Set");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");
  }
}
