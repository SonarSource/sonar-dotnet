/*
 * Sonar .NET Plugin :: Tests
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.tests;

import com.google.common.collect.ImmutableList;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.config.Settings;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CoverageReportImportSensorTest {

  @Test
  public void should_execute_on_project() {
    Project project = mock(Project.class);

    when(project.getLanguageKey()).thenReturn("cs");
    assertThat(new CoverageReportImportSensor(mockSettingsWithNCover3Report()).shouldExecuteOnProject(project)).isTrue();

    when(project.getLanguageKey()).thenReturn("vbnet");
    assertThat(new CoverageReportImportSensor(mockSettingsWithNCover3Report()).shouldExecuteOnProject(project)).isTrue();

    when(project.getLanguageKey()).thenReturn("foo");
    assertThat(new CoverageReportImportSensor(mockSettingsWithNCover3Report()).shouldExecuteOnProject(project)).isFalse();

    when(project.getLanguageKey()).thenReturn("cs");
    assertThat(new CoverageReportImportSensor(mock(Settings.class)).shouldExecuteOnProject(project)).isFalse();
  }

  @Test
  public void should_save_measures() {
    SensorContext context = mock(SensorContext.class);

    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSourceDirs()).thenReturn(ImmutableList.of(new File("src/test/resources/")));

    Project project = mock(Project.class);
    when(project.getFileSystem()).thenReturn(fileSystem);

    new CoverageReportImportSensor(mockSettingsWithNCover3Report()).analyse(project, context);
  }

  private static Settings mockSettingsWithNCover3Report() {
    Settings settings = mock(Settings.class);
    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(true);
    when(settings.getString(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(new File("src/test/resources/ncover3/valid.nccov").getAbsolutePath());
    return settings;
  }

}
