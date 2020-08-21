/*
 * SonarC#
 * Copyright (C) 2014-2020 SonarSource SA
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

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.config.PropertyDefinitions;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.resources.AbstractLanguage;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;

public class CSharpTest {

  private MapSettings settings;
  private CSharp csharp;

  @Before
  public void init() {
    PropertyDefinitions defs = new PropertyDefinitions(
      new CSharpPropertyDefinitions(SonarRuntimeImpl.forSonarQube(Version.create(7, 9), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY)).create());
    settings = new MapSettings(defs);
    csharp = new CSharp(settings.asConfig());
  }

  @Test
  public void shouldGetDefaultFileSuffixes() {
    assertThat(csharp.getFileSuffixes()).containsOnly(".cs");
  }

  @Test
  public void shouldGetCustomFileSuffixes() {
    settings.setProperty(CSharpPlugin.FILE_SUFFIXES_KEY, ".cs,.csharp");
    assertThat(csharp.getFileSuffixes()).containsOnly(".cs", ".csharp");
  }

  @Test
  public void equals_and_hashCode_considers_configuration() {
    MapSettings otherSettings = new MapSettings();
    otherSettings.setProperty("key", "value");
    CSharp otherCSharp = new CSharp(otherSettings.asConfig());
    CSharp sameCSharp = new CSharp(settings.asConfig());
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
      super(CSharpPlugin.LANGUAGE_KEY, CSharpPlugin.LANGUAGE_NAME);
    }

    @Override
    public String[] getFileSuffixes() {
      return new String[0];
    }
  }
}
