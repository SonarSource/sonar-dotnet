/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
package org.sonarsource.csharp.core;

import java.util.Optional;
import org.junit.jupiter.api.Test;
import org.sonar.api.config.Configuration;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

class CSharpLanguageConfigurationTest {

  @Test
  void reads_correct_language() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getStringArray("sonar.cs.roslyn.bugCategories")).thenReturn(new String[]{"C#"});
    when(configuration.getStringArray("sonar.vbnet.roslyn.bugCategories")).thenReturn(new String[]{"VB.NET"});
    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, TestCSharpMetadata.INSTANCE);

    assertThat(config.bugCategories()).containsExactly("C#");
  }

  @Test
  void whenSettingIsTrue_analyzeRazorCode_returnsTrue() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeRazorCode")).thenReturn(Optional.of(true));

    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, TestCSharpMetadata.INSTANCE);

    assertThat(config.analyzeRazorCode()).isTrue();
  }

  @Test
  void whenSettingIsFalse_analyzeRazorCode_returnsFalse() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeRazorCode")).thenReturn(Optional.of(false));

    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, TestCSharpMetadata.INSTANCE);

    assertThat(config.analyzeRazorCode()).isFalse();
  }

  @Test
  void whenSettingIsEmpty_analyzeRazorCode_returnsTrue() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeRazorCode")).thenReturn(Optional.empty());

    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, TestCSharpMetadata.INSTANCE);

    assertThat(config.analyzeRazorCode()).isTrue();
  }
}
