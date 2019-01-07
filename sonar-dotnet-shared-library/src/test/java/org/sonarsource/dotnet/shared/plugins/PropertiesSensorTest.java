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

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import org.junit.Test;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;

import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyNoMoreInteractions;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.mockito.Mockito.when;

public class PropertiesSensorTest {
  private AbstractConfiguration config = mock(AbstractConfiguration.class);
  private ReportPathCollector reportPathCollector = mock(ReportPathCollector.class);

  PropertiesSensor underTest = new PropertiesSensor(config, reportPathCollector, pluginMetadata());

  private DotNetPluginMetadata pluginMetadata() {
    DotNetPluginMetadata metadata = mock(DotNetPluginMetadata.class);
    when(metadata.languageKey()).thenReturn("languageKey");
    when(metadata.shortLanguageName()).thenReturn("Lang Name");
    return metadata;
  }

  @Test
  public void should_collect_properties_from_multiple_modules() {
    Path roslyn1 = Paths.get("roslyn1");
    Path roslyn2 = Paths.get("roslyn2");
    Path proto1 = Paths.get("proto1");
    Path proto2 = Paths.get("proto2");

    when(config.roslynReportPaths()).thenReturn(Collections.singletonList(roslyn1));
    when(config.protobufReportPaths()).thenReturn(Collections.singletonList(proto1));
    underTest.execute(mock(SensorContext.class));
    verify(reportPathCollector).addProtobufDirs(Collections.singletonList(proto1));
    verify(reportPathCollector).addRoslynDirs(Collections.singletonList(new RoslynReport(null, roslyn1)));

    when(config.roslynReportPaths()).thenReturn(Collections.singletonList(roslyn2));
    when(config.protobufReportPaths()).thenReturn(Collections.singletonList(proto2));
    underTest.execute(mock(SensorContext.class));
    verify(reportPathCollector).addProtobufDirs(Collections.singletonList(proto2));
    verify(reportPathCollector).addRoslynDirs(Collections.singletonList(new RoslynReport(null, roslyn2)));

    verifyNoMoreInteractions(reportPathCollector);
  }

  @Test
  public void should_describe() {
    SensorDescriptor desc = mock(SensorDescriptor.class);
    when(desc.name(anyString())).thenReturn(desc);

    underTest.describe(desc);

    verify(desc).name("Lang Name Properties");
    verify(desc).onlyOnLanguage("languageKey");
  }

  @Test
  public void should_continue_if_report_path_not_present() {
    when(config.roslynReportPaths()).thenReturn(Collections.emptyList());
    when(config.protobufReportPaths()).thenReturn(Collections.emptyList());
    underTest.execute(mock(SensorContext.class));
    verifyZeroInteractions(reportPathCollector);
  }
}
