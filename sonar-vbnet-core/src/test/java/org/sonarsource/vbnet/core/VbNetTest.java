/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2025 SonarSource SA
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
package org.sonarsource.vbnet.core;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.sonar.api.config.PropertyDefinitions;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.resources.AbstractLanguage;
import org.sonar.api.utils.System2;
import org.sonarsource.vbnet.core.VbNetCorePluginMetadata.VbNet;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

class VbNetTest {

  private MapSettings settings;
  private VbNetCorePluginMetadata.VbNet vbnet;

  @BeforeEach
  public void init() {
    PropertyDefinitions defs = new PropertyDefinitions(mock(System2.class),
      new VbNetPropertyDefinitions(TestVbNetMetadata.INSTANCE).create());
    settings = new MapSettings(defs);
    vbnet = TestVbNetMetadata.INSTANCE.new VbNet(settings.asConfig());
  }

  @Test
  void shouldGetDefaultFileSuffixes() {
    assertThat(vbnet.getFileSuffixes()).containsOnly(".vb");
  }

  @Test
  void shouldGetCustomFileSuffixes() {
    settings.setProperty(TestVbNetMetadata.INSTANCE.fileSuffixesKey(), ".vb,.vbnet");
    assertThat(vbnet.getFileSuffixes()).containsOnly(".vb", ".vbnet");
  }

  @Test
  void equals_and_hashCode_considers_configuration() {
    MapSettings otherSettings = new MapSettings();
    otherSettings.setProperty("key", "value");
    VbNet otherVbNet = TestVbNetMetadata.INSTANCE.new VbNet(otherSettings.asConfig());
    VbNet sameVbNet =  TestVbNetMetadata.INSTANCE.new VbNet(settings.asConfig());
    FakeVbNet fakeVbNet = new FakeVbNet();

    assertThat(vbnet).isEqualTo(sameVbNet)
      .isNotEqualTo(otherVbNet)
      .isNotEqualTo(fakeVbNet)
      .isNotEqualTo(null)
      .hasSameHashCodeAs(sameVbNet);
    assertThat(vbnet.hashCode()).isNotEqualTo(otherVbNet.hashCode());
  }

  private class FakeVbNet extends AbstractLanguage {

    public FakeVbNet() {
      super( TestVbNetMetadata.INSTANCE.languageKey(), TestVbNetMetadata.INSTANCE.languageName());
    }

    @Override
    public String[] getFileSuffixes() {
      return new String[0];
    }
  }
}
