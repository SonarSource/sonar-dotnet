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
    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, CSharpPlugin.METADATA);

    assertThat(config.bugCategories()).containsExactly("C#");
  }

  @Test
  void whenSettingIsTrue_analyzeRazorCode_returnsTrue() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeRazorCode")).thenReturn(Optional.of(true));

    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, CSharpPlugin.METADATA);

    assertThat(config.analyzeRazorCode()).isTrue();
  }

  @Test
  void whenSettingIsFalse_analyzeRazorCode_returnsFalse() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeRazorCode")).thenReturn(Optional.of(false));

    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, CSharpPlugin.METADATA);

    assertThat(config.analyzeRazorCode()).isFalse();
  }

  @Test
  void whenSettingIsEmpty_analyzeRazorCode_returnsTrue() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.analyzeRazorCode")).thenReturn(Optional.empty());

    CSharpLanguageConfiguration config = new CSharpLanguageConfiguration(configuration, CSharpPlugin.METADATA);

    assertThat(config.analyzeRazorCode()).isTrue();
  }
}
