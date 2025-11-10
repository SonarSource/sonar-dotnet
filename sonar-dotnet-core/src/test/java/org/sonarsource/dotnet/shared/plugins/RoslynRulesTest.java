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

import java.util.List;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatExceptionOfType;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class RoslynRulesTest {

  @Test
  public void rules_loads_data() {
    RoslynRules sut = new RoslynRules(mockMetadata("/RoslynRulesTest"));
    List<RoslynRules.Rule> rules = sut.rules();

    assertThat(rules).hasSize(3);
    RoslynRules.Rule rule = rules.get(0);
    assertThat(rule.id).isEqualTo("S-NO-PARAMS");
    assertThat(rule.parameters).isEmpty();

    rule = rules.get(1);
    assertThat(rule.id).isEqualTo("S-ONE-PARAM");
    assertThat(rule.parameters).hasSize(1);
    assertParameter(rule.parameters[0], "answer", "Answer to life the universe and everything.", "INTEGER", "42");

    rule = rules.get(2);
    assertThat(rule.id).isEqualTo("S-TWO-PARAMS");
    assertThat(rule.parameters).hasSize(2);
    assertParameter(rule.parameters[0], "first", "First description.", "INTEGER", "42");
    assertParameter(rule.parameters[1], "second", "Second description.", "STRING", "2nd default value");
  }

  @Test
  public void test_missing_resource_throws() {
    RoslynRules sut = new RoslynRules(mockMetadata("/org/sonar/plugins/csharp"));
    assertThatExceptionOfType(IllegalStateException.class)
      .isThrownBy(() -> sut.rules())
      .withMessage("Resource does not exist: Rules.json");
  }

  private void assertParameter(RoslynRules.RuleParameter parameter, String key, String description, String type, String defaultValue) {
    assertThat(parameter.key).isEqualTo(key);
    assertThat(parameter.description).isEqualTo(description);
    assertThat(parameter.type).isEqualTo(type);
    assertThat(parameter.defaultValue).isEqualTo(defaultValue);
  }

  private static PluginMetadata mockMetadata(String resourcesDirectory) {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.repositoryKey()).thenReturn("test");
    when(metadata.languageKey()).thenReturn("test");
    when(metadata.resourcesDirectory()).thenReturn(resourcesDirectory);
    return metadata;
  }
}
