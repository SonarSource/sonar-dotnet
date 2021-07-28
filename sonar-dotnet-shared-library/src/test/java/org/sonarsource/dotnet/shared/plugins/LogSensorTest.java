/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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

import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.utils.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class LogSensorTest {
  private static final String LANG_KEY = "LANG_KEY";
  private static final String SHORT_LANG_NAME = "SHORT_LANG_NAME";

  @Rule
  public LogTester logTester = new LogTester();

  private LogSensor sensor;

  @Before
  public void prepare() throws Exception {
    DotNetPluginMetadata metadata = mock(DotNetPluginMetadata.class);
    when(metadata.languageKey()).thenReturn(LANG_KEY);
    when(metadata.shortLanguageName()).thenReturn(SHORT_LANG_NAME);
    sensor = new LogSensor(metadata);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(SHORT_LANG_NAME + " Analysis Log");
    assertThat(sensorDescriptor.languages()).isEmpty();     // should not filter per language
  }
}
