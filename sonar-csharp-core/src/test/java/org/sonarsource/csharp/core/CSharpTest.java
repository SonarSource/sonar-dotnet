/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.sonar.api.config.PropertyDefinitions;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.resources.AbstractLanguage;
import org.sonar.api.utils.System2;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

class CSharpTest {

  private MapSettings settings;
  private CSharpCorePluginMetadata.CSharp csharp;

  @BeforeEach
  public void init() {
    PropertyDefinitions defs = new PropertyDefinitions(mock(System2.class),
      new CSharpPropertyDefinitions(TestCSharpMetadata.INSTANCE).create());
    settings = new MapSettings(defs);
    csharp = TestCSharpMetadata.INSTANCE.new CSharp(settings.asConfig());
  }

  @Test
  void shouldGetDefaultFileSuffixes() {
    assertThat(csharp.getFileSuffixes()).containsOnly(".cs", ".razor");
  }

  @Test
  void shouldGetCustomFileSuffixes() {
    settings.setProperty(TestCSharpMetadata.INSTANCE.fileSuffixesKey(), ".cs,.csharp");
    assertThat(csharp.getFileSuffixes()).containsOnly(".cs", ".csharp");
  }

  @Test
  void equals_and_hashCode_considers_configuration() {
    MapSettings otherSettings = new MapSettings();
    otherSettings.setProperty("key", "value");
    TestCSharpMetadata.CSharp otherCSharp = TestCSharpMetadata.INSTANCE.new CSharp(otherSettings.asConfig());
    TestCSharpMetadata.CSharp sameCSharp = TestCSharpMetadata.INSTANCE.new CSharp(settings.asConfig());
    FakeCSharp fakeCSharp = new FakeCSharp();

    assertThat(csharp).isEqualTo(sameCSharp)
      .isNotEqualTo(otherCSharp)
      .isNotEqualTo(fakeCSharp)
      .isNotEqualTo(null)
      .hasSameHashCodeAs(sameCSharp);
    assertThat(csharp.hashCode()).isNotEqualTo(otherCSharp.hashCode());
  }

  private class FakeCSharp extends AbstractLanguage {

    public FakeCSharp() {
      super(TestCSharpMetadata.INSTANCE.languageKey(), TestCSharpMetadata.INSTANCE.languageName());
    }

    @Override
    public String[] getFileSuffixes() {
      return new String[0];
    }
  }
}
