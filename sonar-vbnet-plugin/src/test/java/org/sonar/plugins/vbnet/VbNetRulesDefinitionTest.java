/*
 * SonarVB
 * Copyright (C) 2012-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
import org.sonarsource.dotnet.shared.plugins.DotNetRulesDefinition;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;

class VbNetRulesDefinitionTest {
  private static final String SECURITY_HOTSPOT_RULE_KEY = "S4792";
  private static final SonarRuntime SONAR_RUNTIME = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER,
    SonarEdition.COMMUNITY);
  private static final RoslynRules ROSLYN_RULES = new RoslynRules(VbNetPlugin.METADATA);

  @Test
  void test() {
    Context context = new Context();
    assertThat(context.repositories()).isEmpty();

    DotNetRulesDefinition definition = new DotNetRulesDefinition(VbNetPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    definition.define(context);

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
  void test_symbolic_execution_rules_are_not_defined() {
    RulesDefinition.Context context = new RulesDefinition.Context();
    assertThat(context.repositories()).isEmpty();

    DotNetRulesDefinition definition = new DotNetRulesDefinition(VbNetPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    definition.define(context);

    assertThat(context.repositories()).hasSize(1);

    assertThat(context.repository("vbnet").rule("S2259")).isNull();
  }

  @Test
  void test_security_hotspot() {
    DotNetRulesDefinition definition = new DotNetRulesDefinition(VbNetPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("vbnet");

    RulesDefinition.Rule hardcodedCredentialsRule = repository.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(hardcodedCredentialsRule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(hardcodedCredentialsRule.activatedByDefault()).isFalse();
  }

  @Test
  void test_security_hotspot_has_correct_type_and_security_standards() {
    DotNetRulesDefinition definition = new DotNetRulesDefinition(VbNetPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("vbnet");

    RulesDefinition.Rule rule = repository.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(rule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder("cwe:117", "cwe:532", "owaspTop10:a10", "owaspTop10:a3", "owaspTop10" +
      "-2021:a9");
  }

  @Test
  void test_all_rules_have_status_set() {
    DotNetRulesDefinition definition = new DotNetRulesDefinition(VbNetPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("vbnet");

    for (RulesDefinition.Rule rule : repository.rules()) {
      assertThat(rule.status()).isNotNull();
    }
  }
}
