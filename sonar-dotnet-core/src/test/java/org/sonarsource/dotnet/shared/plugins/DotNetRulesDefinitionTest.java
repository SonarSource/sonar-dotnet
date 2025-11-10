/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.InputStream;
import java.util.Set;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.rule.RuleStatus;
import org.sonar.api.rules.RuleType;
import org.sonar.api.server.debt.DebtRemediationFunction;
import org.sonar.api.server.rule.RuleParamType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class DotNetRulesDefinitionTest {
  private static final String PCI_DSS_RULE_KEY = "S1115";
  private static final String OWASP_ASVS_RULE_KEY = "S1116";
  private static final String STIG_RULE_KEY = "S1117";
  private static final String TAGS_RULE_KEY = "S1111";
  private static final String MINIMAL_RULE_KEY = "S100";
  private static final String SINGLE_PARAM_RULE_KEY = "S1113";
  private static final String MULTI_PARAM_RULE_KEY = "S1112";
  private static final String SECURITY_HOTSPOT_RULE_KEY = "S4502";
  private static final String VULNERABILITY_RULE_KEY = "S2115";
  private static final PluginMetadata METADATA = mockMetadata();
  private static final SonarRuntime SONAR_RUNTIME = SonarRuntimeImpl.forSonarQube(Version.create(10, 10), SonarQubeSide.SCANNER,
    SonarEdition.COMMUNITY);
  private static final RoslynRules ROSLYN_RULES = new TestRoslynRules();
  private static final RulesDefinition.Context CONTEXT = new RulesDefinition.Context();

  private static RulesDefinition.Repository ruleRepo;

  @BeforeClass
  public static void setupContext() {
    DotNetRulesDefinition definition = new DotNetRulesDefinition(METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    definition.define(CONTEXT);
    ruleRepo = CONTEXT.repository("test");
  }

  @Test
  public void nonSonarWayRule_disabledByDefault() {
    RulesDefinition.Rule rule = ruleRepo.rule("S1117");
    assertThat(rule).isNotNull();
    assertThat(rule.activatedByDefault()).isFalse();
  }

  @Test
  public void rule_properties_are_loaded() {
    assertThat(CONTEXT.repositories()).hasSize(1);

    RulesDefinition.Rule s100 = ruleRepo.rule("S100");
    assertThat(s100).isNotNull();
    assertThat(s100.activatedByDefault()).isTrue();
    assertThat(s100.name()).isEqualTo("Methods and properties should be named in PascalCase");
    assertThat(s100.type()).isEqualTo(RuleType.CODE_SMELL);
    assertThat(s100.status()).isEqualTo(RuleStatus.READY);
    assertThat(s100.severity()).isEqualTo("MINOR");
    assertThat(s100.debtRemediationFunction().type()).isEqualTo(DebtRemediationFunction.Type.CONSTANT_ISSUE);
    assertThat(s100.debtRemediationFunction().baseEffort()).isEqualTo("5min");
    assertThat(s100.params()).isEmpty();
    assertThat(s100.tags()).isEmpty();
  }

  @Test
  public void securityStandards_9_5_PCI_DSS_isSet() {
    assertThat(getSecurityStandards(Version.create(9, 5), PCI_DSS_RULE_KEY))
      .containsExactlyInAnyOrder("pciDss-3.2:6.5.10", "pciDss-4.0:6.2.4");
  }

  @Test
  public void securityStandards_9_9_ASVS_isSet() {
    assertThat(getSecurityStandards(Version.create(9, 9), OWASP_ASVS_RULE_KEY))
      .containsExactlyInAnyOrder("owaspAsvs-4.0:2.10.4", "owaspAsvs-4.0:3.5.2", "owaspAsvs-4.0:6.4.1");
  }

  @Test
  public void securityStandards_10_10_STIG_isSet() {
    assertThat(getSecurityStandards(Version.create(10, 10), STIG_RULE_KEY))
      .containsExactlyInAnyOrder("stig-ASD_V5R3:V-222542", "stig-ASD_V5R3:V-222603");
  }

  @Test
  public void tags_areSet() {
    RulesDefinition.Rule rule = ruleRepo.rule(TAGS_RULE_KEY);
    assertThat(rule.tags()).containsExactlyInAnyOrder("cwe", "owasp-a10", "sans-top25-porous", "owasp-a3");
  }

  @Test
  public void tags_areEmpty() {
    RulesDefinition.Rule rule = ruleRepo.rule(MINIMAL_RULE_KEY);
    assertThat(rule.tags()).isEmpty();
  }

  @Test
  public void remediation_isSet() {
    assertThat(ruleRepo.rule("S1111").debtRemediationFunction())
      .hasToString("DebtRemediationFunction{type=CONSTANT_ISSUE, gap multiplier=null, base effort=5min}");
    assertThat(ruleRepo.rule("S1112").debtRemediationFunction())
      .hasToString("DebtRemediationFunction{type=LINEAR, gap multiplier=10min, base effort=null}");
    assertThat(ruleRepo.rule("S1113").debtRemediationFunction())
      .hasToString("DebtRemediationFunction{type=LINEAR_OFFSET, gap multiplier=30min, base effort=4h}");
    assertThat(ruleRepo.rule("S1114").debtRemediationFunction()).isNull();
  }

  @Test
  public void noParams_paramsIsEmpty() {
    RulesDefinition.Rule rule = ruleRepo.rule(MINIMAL_RULE_KEY);
    assertThat(rule.params()).isEmpty();
  }

  @Test
  public void singleParam_isSet() {
    RulesDefinition.Rule rule = ruleRepo.rule(SINGLE_PARAM_RULE_KEY);
    assertThat(rule.params()).hasSize(1);
    assertParam(rule.params().get(0), "answer", RuleParamType.INTEGER, "42", "Answer to life the universe and everything.");
  }

  @Test
  public void multipleParams_areSet() {
    RulesDefinition.Rule rule = ruleRepo.rule(MULTI_PARAM_RULE_KEY);
    assertThat(rule.params()).hasSize(2);
    assertParam(rule.params().get(0), "random", RuleParamType.INTEGER, "4", "chosen by fair dice roll. guaranteed to be random.");
    assertParam(rule.params().get(1), "Tr0ub4dor&3", RuleParamType.STRING, "correct horse battery staple", "ELI5 entropy please!");
  }

  @Test
  public void securityHotspot_isActivatedByDefault() {
    RulesDefinition.Rule hardcodedCredentialsRule = ruleRepo.rule(SECURITY_HOTSPOT_RULE_KEY);
    assertThat(hardcodedCredentialsRule.type()).isEqualTo(RuleType.SECURITY_HOTSPOT);
    assertThat(hardcodedCredentialsRule.activatedByDefault()).isTrue();
  }

  @Test
  public void securityHotspot_hasSecurityStandards() {
    RulesDefinition.Rule rule = ruleRepo.rule(SECURITY_HOTSPOT_RULE_KEY);
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
  public void vulnerability_hasSecurityStandards() {
    RulesDefinition.Rule rule = ruleRepo.rule(VULNERABILITY_RULE_KEY);
    assertThat(rule.type()).isEqualTo(RuleType.VULNERABILITY);
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder(
      "cwe:521",
      "owaspAsvs-4.0:9.2.2",
      "owaspAsvs-4.0:9.2.3",
      "owaspTop10:a2",
      "owaspTop10:a3",
      "owaspTop10-2021:a7",
      "pciDss-3.2:6.5.10",
      "pciDss-4.0:6.2.4");
  }

  private static void assertParam(RulesDefinition.Param param, String expectedKey, RuleParamType expectedType,
    String expectedDefaultValue, String expectedDescription) {
    assertThat(param.key()).isEqualTo(expectedKey);
    assertThat(param.type()).isEqualTo(expectedType);
    assertThat(param.defaultValue()).isEqualTo(expectedDefaultValue);
    assertThat(param.description()).isEqualTo(expectedDescription);
  }

  private static Set<String> getSecurityStandards(Version version, String ruleId) {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(version, SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    DotNetRulesDefinition sut = new DotNetRulesDefinition(METADATA, sonarRuntime, ROSLYN_RULES);
    var securityContext = new RulesDefinition.Context();
    sut.define(securityContext);
    RulesDefinition.Repository repository = securityContext.repository("test");

    assertThat(repository).isNotNull();
    RulesDefinition.Rule rule = repository.rule(ruleId);
    assertThat(rule).isNotNull();
    return rule.securityStandards();
  }

  private static PluginMetadata mockMetadata() {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.repositoryKey()).thenReturn("test");
    when(metadata.languageKey()).thenReturn("test");
    when(metadata.resourcesDirectory()).thenReturn("/DotNetRulesDefinitionTest/");
    return metadata;
  }

  private static class TestRoslynRules extends RoslynRules {

    TestRoslynRules() {
      super(METADATA);
    }

    @Override
    InputStream getResourceAsStream(String name) {
      return DotNetRulesDefinitionTest.class.getResourceAsStream(name);
    }
  }
}
