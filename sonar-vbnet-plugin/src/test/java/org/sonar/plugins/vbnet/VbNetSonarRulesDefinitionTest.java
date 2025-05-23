/*
 * SonarVB
 * Copyright (C) 2012-2024 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.junit.jupiter.api.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.rule.RuleStatus;
import org.sonar.api.rules.RuleType;
import org.sonar.api.server.debt.DebtRemediationFunction;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinition.Context;
import org.sonar.api.server.rule.RulesDefinition.Rule;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;

class VbNetSonarRulesDefinitionTest {
  private static final String SECURITY_HOTSPOT_RULE_KEY = "S4792";
  private static final SonarRuntime SONAR_RUNTIME = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);

  @Test
  void test() {
    Context context = new Context();
    assertThat(context.repositories()).isEmpty();

    VbNetSonarRulesDefinition vbnetRulesDefinition = new VbNetSonarRulesDefinition(SONAR_RUNTIME);
    vbnetRulesDefinition.define(context);

    assertThat(context.repositories()).hasSize(1);
    assertThat(context.repository("vbnet").rules()).isNotEmpty();

    Rule s1197 = context.repository("vbnet").rule("S1197");

    assertThat(s1197.name()).isEqualTo("Array designators \"()\" should be on the type, not the variable");
    assertThat(s1197.type()).isEqualTo(RuleType.CODE_SMELL);
    assertThat(s1197.status()).isEqualTo(RuleStatus.READY);
    assertThat(s1197.severity()).isEqualTo("MINOR");
    assertThat(s1197.debtRemediationFunction().type()).isEqualTo(DebtRemediationFunction.Type.CONSTANT_ISSUE);
    assertThat(s1197.debtRemediationFunction().baseEffort()).isEqualTo("5min");
    assertThat(s1197.params()).isEmpty();
    assertThat(s1197.tags()).hasSize(1).containsExactly("convention");
  }

  @Test
  void test_security_hotspot() {
    VbNetSonarRulesDefinition definition = new VbNetSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("vbnet");

    RulesDefinition.Rule hardcodedCredentialsRule = repository.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(hardcodedCredentialsRule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(hardcodedCredentialsRule.activatedByDefault()).isFalse();
  }

  @Test
  void test_security_hotspot_has_correct_type_and_security_standards() {
    VbNetSonarRulesDefinition definition = new VbNetSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("vbnet");

    RulesDefinition.Rule rule = repository.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(rule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder("cwe:117", "cwe:532", "owaspTop10:a10", "owaspTop10:a3", "owaspTop10-2021:a9");
  }

  @Test
  void test_all_rules_have_status_set() {
    VbNetSonarRulesDefinition definition = new VbNetSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("vbnet");

    for (RulesDefinition.Rule rule : repository.rules()) {
      assertThat(rule.status()).isNotNull();
    }
  }
}
