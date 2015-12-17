/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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

import org.junit.Test;
import org.sonar.api.server.rule.RulesDefinition.Context;

import java.util.Set;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpSonarRulesDefinitionTest {

  @Test
  public void test() {
    Context context = new Context();
    assertThat(context.repositories()).isEmpty();

    CSharpSonarRulesDefinition csharpRulesDefinition = new CSharpSonarRulesDefinition();
    csharpRulesDefinition.define(context);

    assertThat(context.repositories()).hasSize(1);
    assertThat(context.repository("csharpsquid").rules()).isNotEmpty();

    Set<String> sonarLintRules = csharpRulesDefinition.allRuleKeys();
    assertThat(sonarLintRules.contains("S100")).isTrue();
    assertThat(sonarLintRules.size()).isGreaterThan(50);
    assertThat(sonarLintRules.size()).isLessThanOrEqualTo(context.repository("csharpsquid").rules().size());
  }

}
