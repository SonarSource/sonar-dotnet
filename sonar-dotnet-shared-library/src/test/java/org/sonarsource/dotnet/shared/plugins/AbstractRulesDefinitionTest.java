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

import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class AbstractRulesDefinitionTest {
  @Test
  public void constructor_with_null() {
    TestRulesDefinition test = new TestRulesDefinition();
    assertThat(test).isNotNull();
  }

  private static class TestRulesDefinition extends AbstractRulesDefinition {
    TestRulesDefinition() {
      super("test", "test", "test", "test");
    }

    @Override
    protected String getRuleJson(String ruleKey) {
      return "/org/sonar/plugins/test/" + ruleKey + "_test.json";
    }
  }
}

