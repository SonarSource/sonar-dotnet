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

import java.util.Optional;
import org.junit.Test;
import org.sonar.api.config.Configuration;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class AbstractLanguageConfigurationTest {

  @Test
  public void ignoreExternalIssues_is_false_by_default() {
    Configuration configuration = mock(Configuration.class);
    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.ignoreThirdPartyIssues()).isFalse();
  }

  @Test
  public void ignoreExternalIssues_is_true_when_set() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.roslyn.ignoreIssues")).thenReturn(Optional.of(true));
    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.ignoreThirdPartyIssues()).isTrue();
  }

  @Test
  public void bugCategories_reads_configuration() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getStringArray("sonar.cs.roslyn.bugCategories")).thenReturn(new String[]{"A", "B", "C"});
    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.bugCategories()).containsExactly("A", "B", "C");
  }

  @Test
  public void codeSmellCategories_reads_configuration() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getStringArray("sonar.cs.roslyn.codeSmellCategories")).thenReturn(new String[]{"A", "B", "C"});
    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.codeSmellCategories()).containsExactly("A", "B", "C");
  }

  @Test
  public void vulnerabilityCategories_reads_configuration() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getStringArray("sonar.cs.roslyn.vulnerabilityCategories")).thenReturn(new String[]{"A", "B", "C"});
    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.vulnerabilityCategories()).containsExactly("A", "B", "C");
  }

  @Test
  public void whenSettingIsTrue_analyzeGeneratedCode_returnsTrue() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeGeneratedCode")).thenReturn(Optional.of(true));

    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.analyzeGeneratedCode()).isTrue();
  }

  @Test
  public void whenSettingIsFalse_analyzeGeneratedCode_returnsFalse() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeGeneratedCode")).thenReturn(Optional.of(false));

    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.analyzeGeneratedCode()).isFalse();
  }

  @Test
  public void whenSettingIsEmpty_analyzeGeneratedCode_returnsFalse() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeGeneratedCode")).thenReturn(Optional.empty());

    AbstractLanguageConfiguration config = createConfiguration(configuration);

    assertThat(config.analyzeGeneratedCode()).isFalse();
  }

  private AbstractLanguageConfiguration createConfiguration(Configuration configuration) {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageKey()).thenReturn("cs");
    return new AbstractLanguageConfiguration(configuration, metadata) {
    };
  }
}
