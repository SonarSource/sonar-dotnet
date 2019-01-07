/*
 * SonarVB
 * Copyright (C) 2012-2019 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.config.PropertyDefinitions;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;

public class VbNetTest {

  private MapSettings settings;
  private VbNet vbnet;

  @Before
  public void init() {
    PropertyDefinitions defs = new PropertyDefinitions(new VbNetPropertyDefinitions(SonarRuntimeImpl.forSonarQube(Version.create(7, 4), SonarQubeSide.SCANNER)).create());
    settings = new MapSettings(defs);
    vbnet = new VbNet(settings.asConfig());
  }

  @Test
  public void shouldGetDefaultFileSuffixes() {
    assertThat(vbnet.getFileSuffixes()).containsOnly(".vb");
  }

  @Test
  public void shouldGetCustomFileSuffixes() {
    settings.setProperty(VbNetPlugin.FILE_SUFFIXES_KEY, ".vb,.vbnet");
    assertThat(vbnet.getFileSuffixes()).containsOnly(".vb", ".vbnet");
  }

}
