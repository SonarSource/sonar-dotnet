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

import java.util.Arrays;
import java.util.HashSet;
import org.junit.Test;
import org.mockito.Mockito;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;

public class AbstractRulesDefinitionTest {

  @Test
  public void test() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 3), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    AbstractRulesDefinition test = org.mockito.Mockito.spy(new TestRulesDefinition(sonarRuntime));
    Mockito.doReturn(TestRulesDefinition.class.getResourceAsStream("/AbstractRulesDefinitionTest/S1111.json")).when(test).getResourceAsStream(any());
    RulesDefinition.Context context = new RulesDefinition.Context();
    test.define(context);

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
    AbstractRulesDefinition test = org.mockito.Mockito.spy(new TestRulesDefinition(sonarRuntime));
    Mockito.doReturn(TestRulesDefinition.class.getResourceAsStream("/AbstractRulesDefinitionTest/S1111.json")).when(test).getResourceAsStream(any());
    RulesDefinition.Context context = new RulesDefinition.Context();
    test.define(context);

    RulesDefinition.Repository repository = context.repository("test");
    assertThat(repository).isNotNull();

    RulesDefinition.Rule rule = repository.rule("S1111");
    assertThat(rule).isNotNull();
    assertThat(rule.securityStandards()).containsExactlyInAnyOrder("cwe:117", "cwe:532", "owaspTop10:a10", "owaspTop10:a3");
  }

  private static class TestRulesDefinition extends AbstractRulesDefinition {
    TestRulesDefinition(SonarRuntime runtime) {
      super("test", "test", runtime, "/AbstractRulesDefinitionTest/", "", new HashSet<String>(Arrays.asList("S1111")));
    }
  }
}
