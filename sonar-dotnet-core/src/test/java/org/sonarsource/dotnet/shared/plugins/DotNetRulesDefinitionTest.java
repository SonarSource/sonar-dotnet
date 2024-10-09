/*
 * SonarSource :: .NET :: Core
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.InputStream;
import java.util.Set;
import org.junit.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatExceptionOfType;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class DotNetRulesDefinitionTest {

  private static final String PCI_DSS_RULE_KEY = "S1115";
  private static final String OWASP_ASVS_RULE_KEY = "S1116";
  private static final String STIG_RULE_KEY = "S1117";
  private static final SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER,
    SonarEdition.COMMUNITY);

  @Test
  public void test() {
    DotNetRulesDefinition sut = new TestRulesDefinition(sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();
    sut.define(context);

    RulesDefinition.Repository repository = context.repository("test");
    assertThat(repository).isNotNull();

    RulesDefinition.Rule rule = repository.rule("S1111");
    assertThat(rule).isNotNull();
    assertThat(rule.securityStandards())
      .containsExactlyInAnyOrder("cwe:117", "cwe:532", "owaspTop10:a10", "owaspTop10:a3", "owaspTop10-2021:a9");
  }

  @Test
  public void test_security_standards_9_5_PCI_DSS_is_available() {
    assertThat(getSecurityStandards(Version.create(9, 5), PCI_DSS_RULE_KEY))
      .containsExactlyInAnyOrder("pciDss-3.2:6.5.10", "pciDss-4.0:6.2.4");
  }

  @Test
  public void test_security_standards_9_9_ASVS_is_available() {
    assertThat(getSecurityStandards(Version.create(9, 9), OWASP_ASVS_RULE_KEY))
      .containsExactlyInAnyOrder("owaspAsvs-4.0:2.10.4", "owaspAsvs-4.0:3.5.2", "owaspAsvs-4.0:6.4.1");
  }

  @Test
  public void test_security_standards_STIG_is_available() {
    assertThat(getSecurityStandards(Version.create(10, 10), STIG_RULE_KEY))
      .containsExactlyInAnyOrder("stig-ASD_V5R3:V-222542", "stig-ASD_V5R3:V-222603");
  }

  @Test
  public void test_remediation_is_set() {
    DotNetRulesDefinition sut = new TestRulesDefinition(sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();
    sut.define(context);

    RulesDefinition.Repository repository = context.repository("test");

    assertThat(repository.rule("S1111").debtRemediationFunction())
      .hasToString("DebtRemediationFunction{type=CONSTANT_ISSUE, gap multiplier=null, base effort=5min}");
    assertThat(repository.rule("S1112").debtRemediationFunction())
      .hasToString("DebtRemediationFunction{type=LINEAR, gap multiplier=10min, base effort=null}");
    assertThat(repository.rule("S1113").debtRemediationFunction())
      .hasToString("DebtRemediationFunction{type=LINEAR_OFFSET, gap multiplier=30min, base effort=4h}");
    assertThat(repository.rule("S1114").debtRemediationFunction()).isNull();
  }

  @Test
  public void test_missing_resource_throws() {
    PluginMetadata metadata = mockMetadata();
    when(metadata.resourcesDirectory()).thenReturn("/org/sonar/plugins/csharp");
    DotNetRulesDefinition sut = new DotNetRulesDefinition(metadata, sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();

    assertThatExceptionOfType(IllegalStateException.class)
      .isThrownBy(() -> sut.define(context))
      .withMessage("Resource does not exist: Rules.json");
  }

  private static Set<String> getSecurityStandards(Version version, String ruleId) {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(version, SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    DotNetRulesDefinition sut = new TestRulesDefinition(sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();
    sut.define(context);

    RulesDefinition.Repository repository = context.repository("test");
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

  private static class TestRulesDefinition extends DotNetRulesDefinition {

    TestRulesDefinition(SonarRuntime runtime) {
      super(mockMetadata(), runtime);
    }

    @Override
    InputStream getResourceAsStream(String name) {
      return DotNetRulesDefinitionTest.class.getResourceAsStream(name);
    }
  }
}
