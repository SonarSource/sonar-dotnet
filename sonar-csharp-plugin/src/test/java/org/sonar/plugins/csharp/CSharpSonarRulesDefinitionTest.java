/*
 * SonarC#
 * Copyright (C) 2014-2024 SonarSource SA
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

import org.junit.jupiter.api.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.rule.RuleStatus;
import org.sonar.api.rules.RuleType;
import org.sonar.api.server.debt.DebtRemediationFunction;
import org.sonar.api.server.rule.RuleParamType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinition.Context;
import org.sonar.api.server.rule.RulesDefinition.Rule;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;

class CSharpSonarRulesDefinitionTest {
  private static final String SECURITY_HOTSPOT_RULE_KEY = "S4502";
  private static final String VULNERABILITY_RULE_KEY = "S2053";
  private static final String NO_TAGS_RULE_KEY = "S1048";
  private static final String SINGLE_PARAM_RULE_KEY = "S1200";
  private static final String MULTI_PARAM_RULE_KEY = "S110";

  private static final SonarRuntime SONAR_RUNTIME = SonarRuntimeImpl.forSonarQube(Version.create(10, 10), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);

  @Test
  void test() {
    Context context = new Context();
    assertThat(context.repositories()).isEmpty();

    CSharpSonarRulesDefinition csharpRulesDefinition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    csharpRulesDefinition.define(context);

    assertThat(context.repositories()).hasSize(1);
    assertThat(context.repository("csharpsquid").rules()).isNotEmpty();

    Rule s100 = context.repository("csharpsquid").rule("S100");
    assertThat(s100.name()).isEqualTo("Methods and properties should be named in PascalCase");
    assertThat(s100.type()).isEqualTo(RuleType.CODE_SMELL);
    assertThat(s100.status()).isEqualTo(RuleStatus.READY);
    assertThat(s100.severity()).isEqualTo("MINOR");
    assertThat(s100.debtRemediationFunction().type()).isEqualTo(DebtRemediationFunction.Type.CONSTANT_ISSUE);
    assertThat(s100.debtRemediationFunction().baseEffort()).isEqualTo("5min");
    assertThat(s100.params()).isEmpty();
    assertThat(s100.tags()).hasSize(1).containsExactly("convention");
  }

  @Test
  void test_security_hotspot() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("csharpsquid");

    RulesDefinition.Rule hardcodedCredentialsRule = repository.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(hardcodedCredentialsRule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(hardcodedCredentialsRule.activatedByDefault()).isFalse();
  }

  @Test
  void test_security_hotspot_has_correct_type_and_security_standards() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("csharpsquid");

    RulesDefinition.Rule rule = repository.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(rule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder(
      "cwe:352",
      "owaspTop10:a6",
      "owaspTop10-2021:a1",
      "pciDss-3.2:6.5.9",
      "pciDss-4.0:6.2.4",
      "owaspAsvs-4.0:13.2.3",
      "owaspAsvs-4.0:4.2.2",
      "stig-ASD_V5R3:V-222603");
  }

  @Test
  void test_security_standards_with_vulnerability() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("csharpsquid");

    RulesDefinition.Rule rule = repository.rule(VULNERABILITY_RULE_KEY);
    assertThat(rule.type()).isEqualTo(RuleType.VULNERABILITY);
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder(
      "cwe:759",
      "cwe:760",
      "owaspTop10:a3",
      "owaspTop10-2021:a2",
      "pciDss-3.2:6.5.10",
      "pciDss-4.0:6.2.4",
      "stig-ASD_V5R3:V-222542");
  }

  @Test
  void test_all_rules_have_metadata_set() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("csharpsquid");

    for (RulesDefinition.Rule rule : repository.rules()) {
      assertThat(rule.name()).isNotEmpty();
      assertThat(rule.type()).isNotNull();
      assertThat(rule.status()).isNotNull();
      assertThat(rule.severity()).isNotEmpty();
    }
  }

  @Test
  void test_all_rules_have_htmldescription() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("csharpsquid");

    for (RulesDefinition.Rule rule : repository.rules()) {
      assertThat(rule.htmlDescription()).isNotEmpty().hasSizeGreaterThan(100);
    }
  }

  @Test
  void test_tags_are_set() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Rule rule = context.repository("csharpsquid").rule(SECURITY_HOTSPOT_RULE_KEY);

    assertThat(rule.tags()).containsExactly("cwe");
  }

  @Test
  void test_tags_are_empty() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Rule rule = context.repository("csharpsquid").rule(NO_TAGS_RULE_KEY);

    assertThat(rule.tags()).isEmpty();
  }

  @Test
  void test_remediation_is_set() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Repository repository = context.repository("csharpsquid");

    // We don't have rule with Linear to assert. That path is tested in AbstractRulesDefinition.test_remediation_is_set_linear()
    assertThat(repository.rule("S100").debtRemediationFunction()).hasToString("DebtRemediationFunction{type=CONSTANT_ISSUE, gap multiplier=null, base effort=5min}");
    assertThat(repository.rule("S110").debtRemediationFunction()).hasToString("DebtRemediationFunction{type=LINEAR_OFFSET, gap multiplier=30min, base effort=4h}");
  }

  @Test
  void test_no_params() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Rule rule = context.repository("csharpsquid").rule(SECURITY_HOTSPOT_RULE_KEY);

    assertThat(rule.params()).isEmpty();
  }

  @Test
  void test_single_params() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Rule rule = context.repository("csharpsquid").rule(SINGLE_PARAM_RULE_KEY);

    assertThat(rule.params()).hasSize(1);
    assertParam(rule.params().get(0), "max", RuleParamType.INTEGER, "30", "Maximum number of types a single type is allowed to depend upon");
  }

  @Test
  void test_multiple_params() {
    CSharpSonarRulesDefinition definition = new CSharpSonarRulesDefinition(SONAR_RUNTIME);
    RulesDefinition.Context context = new RulesDefinition.Context();
    definition.define(context);
    RulesDefinition.Rule rule = context.repository("csharpsquid").rule(MULTI_PARAM_RULE_KEY);

    assertThat(rule.params()).hasSize(2);
    assertParam(rule.params().get(0), "max", RuleParamType.INTEGER, "5", "Maximum depth of the inheritance tree. (Number)");
    assertParam(rule.params().get(1), "filteredClasses", RuleParamType.STRING, null,
      "Comma-separated list of classes or records to be filtered out of the count of inheritance. Depth counting will stop when a filtered class or record is reached. For example: System.Windows.Controls.UserControl, System.Windows.*. (String)");
  }

  private static void assertParam(RulesDefinition.Param param, String expectedKey, RuleParamType expectedType, String expectedDefaultValue, String expectedDescription) {
    assertThat(param.key()).isEqualTo(expectedKey);
    assertThat(param.type()).isEqualTo(expectedType);
    assertThat(param.defaultValue()).isEqualTo(expectedDefaultValue);
    assertThat(param.description()).isEqualTo(expectedDescription);
  }
}
