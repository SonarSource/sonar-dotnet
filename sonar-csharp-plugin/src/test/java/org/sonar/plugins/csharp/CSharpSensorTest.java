/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.component.ResourcePerspectives;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CSharpSensorTest {

  private SensorContext context;
  private DefaultInputFile inputFile;
  private DefaultFileSystem fs;
  private FileLinesContext fileLinesContext;
  private FileLinesContextFactory fileLinesContextFactory;
  private NSonarQubeAnalyzerExtractor extractor;

  @Test
  public void shouldExecuteOnProject() {
    DefaultFileSystem fs = new DefaultFileSystem();

    CSharpSensor sensor =
      new CSharpSensor(
        mock(Settings.class), mock(NSonarQubeAnalyzerExtractor.class),
        fs,
        mock(FileLinesContextFactory.class), mock(NoSonarFilter.class), mock(RulesProfile.class), mock(ResourcePerspectives.class));

    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("foo").setAbsolutePath("foo").setLanguage("java"));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("bar").setAbsolutePath("bar").setLanguage("cs"));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isTrue();
  }

  @Before
  public void init() {
    fs = new DefaultFileSystem();
    fs.setWorkDir(new File("src/test/resources/CSharpSensorTest"));

    inputFile = new DefaultInputFile("MyClass.cs").setAbsolutePath("MyClass.cs");
    fs.add(inputFile);

    fileLinesContext = mock(FileLinesContext.class);
    fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(inputFile)).thenReturn(fileLinesContext);

    extractor = mock(NSonarQubeAnalyzerExtractor.class);
    when(extractor.executableFile()).thenReturn(new File("src/test/resources/CSharpSensorTest/fake.bat"));

    CSharpSensor sensor =
      new CSharpSensor(
        mock(Settings.class), extractor,
        fs,
        fileLinesContextFactory, mock(NoSonarFilter.class), mock(RulesProfile.class), mock(ResourcePerspectives.class));

    context = mock(SensorContext.class);
    sensor.analyse(mock(Project.class), context);
  }

  @Test
  public void metrics() {
    verify(context).saveMeasure(inputFile, CoreMetrics.LINES, 27d);
    verify(context).saveMeasure(inputFile, CoreMetrics.CLASSES, 1d);
    verify(context).saveMeasure(inputFile, CoreMetrics.ACCESSORS, 5d);
    verify(context).saveMeasure(inputFile, CoreMetrics.STATEMENTS, 2d);
    verify(context).saveMeasure(inputFile, CoreMetrics.FUNCTIONS, 3d);
    verify(context).saveMeasure(inputFile, CoreMetrics.PUBLIC_API, 4d);
    verify(context).saveMeasure(inputFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API, 2d);
    verify(context).saveMeasure(inputFile, CoreMetrics.COMPLEXITY, 3d);
  }

}
