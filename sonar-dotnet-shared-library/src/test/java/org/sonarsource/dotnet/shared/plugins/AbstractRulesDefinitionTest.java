/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2022 SonarSource SA
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
import org.junit.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatExceptionOfType;

public class AbstractRulesDefinitionTest {

  @Test
  public void test() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    AbstractRulesDefinition sut = new TestRulesDefinition(sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();
    sut.define(context);

    RulesDefinition.Repository repository = context.repository("test");
    assertThat(repository).isNotNull();

    RulesDefinition.Rule rule = repository.rule("S1111");
    assertThat(rule).isNotNull();
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder("cwe:117", "cwe:532", "owaspTop10:a10", "owaspTop10:a3",
      "owaspTop10-2021:a9");
  }

  @Test
  public void test_before_9_3() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 2), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    AbstractRulesDefinition sut = new TestRulesDefinition(sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();
    sut.define(context);

    RulesDefinition.Repository repository = context.repository("test");
    assertThat(repository).isNotNull();

    RulesDefinition.Rule rule = repository.rule("S1111");
    assertThat(rule).isNotNull();
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder("cwe:117", "cwe:532", "owaspTop10:a10", "owaspTop10:a3");
  }

  @Test
  public void test_remediation_is_set() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    AbstractRulesDefinition sut = new TestRulesDefinition(sonarRuntime);
    RulesDefinition.Context context = new RulesDefinition.Context();
    sut.define(context);

    RulesDefinition.Repository repository = context.repository("test");

    assertThat(repository.rule("S1111").debtRemediationFunction()).hasToString("DebtRemediationFunction{type=CONSTANT_ISSUE, gap multiplier=null, base effort=5min}");
    assertThat(repository.rule("S1112").debtRemediationFunction()).hasToString("DebtRemediationFunction{type=LINEAR, gap multiplier=10min, base effort=null}");
    assertThat(repository.rule("S1113").debtRemediationFunction()).hasToString("DebtRemediationFunction{type=LINEAR_OFFSET, gap multiplier=30min, base effort=4h}");

  }

  @Test
  public void test_missing_resource_throws() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    AbstractRulesDefinition sut = new AbstractRulesDefinition("test", "test", sonarRuntime, "/Nonexistent/", "") {
    };
    RulesDefinition.Context context = new RulesDefinition.Context();

    assertThatExceptionOfType(IllegalStateException.class)
      .isThrownBy(() -> sut.define(context))
      .withMessage("Resource does not exist: Rules.json");
  }

  @Test
  public void test_missing_remediation_throws() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    AbstractRulesDefinition sut = new TestRulesDefinition(sonarRuntime, "MissingRemediation/");
    RulesDefinition.Context context = new RulesDefinition.Context();

    assertThatExceptionOfType(IllegalStateException.class)
      .isThrownBy(() -> sut.define(context))
      .withMessage("Rspec is missing remediation: S0001");
  }

  private static class TestRulesDefinition extends AbstractRulesDefinition {
    TestRulesDefinition(SonarRuntime runtime) {
      this(runtime, "");
    }

    TestRulesDefinition(SonarRuntime runtime, String suffix) {
      super("test", "test", runtime, "/AbstractRulesDefinitionTest/" + suffix, "");
    }

    @Override
    InputStream getResourceAsStream(String name) {
      return AbstractRulesDefinitionTest.class.getResourceAsStream(name);
    }
  }
}
