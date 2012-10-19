/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop;

import com.google.common.collect.Lists;
import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.config.Settings;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.stylecop.profiles.StyleCopProfileExporter;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.dotnet.api.visualstudio.VisualStudioSolution;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Collections;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.doAnswer;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class StyleCopSensorTest {

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private RulesProfile rulesProfile;
  private DotNetConfiguration conf;
  private Settings settings;
  private StyleCopSensor sensor;
  private Project project;

  @Before
  public void init() {
    project = mock(Project.class);
    vsProject = mock(VisualStudioProject.class);
    when(vsProject.getName()).thenReturn("Project #1");
    solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject));
    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);

    settings = Settings.createForComponent(new StyleCopPlugin());
    conf = new DotNetConfiguration(settings);

    rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository(anyString()))
        .thenReturn(Collections.singletonList(new ActiveRule()));

  }

  private void initializeSensor() {
    sensor = new StyleCopSensor.RegularStyleCopSensor(null, rulesProfile, mock(StyleCopProfileExporter.RegularStyleCopProfileExporter.class), null, conf,
        microsoftWindowsEnvironment);
  }

  private void initializeTestSensor() {
    sensor = new StyleCopSensor.UnitTestsStyleCopSensor(null, rulesProfile, mock(StyleCopProfileExporter.UnitTestsStyleCopProfileExporter.class), null, conf,
        microsoftWindowsEnvironment);
  }

  @Test
  public void testShouldExecuteOnProject() throws Exception {
    initializeSensor();
    Project project = mock(Project.class);
    when(project.getLanguageKey()).thenReturn("cs");
    when(project.getName()).thenReturn("Project #1");
    assertTrue(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnSkippedProject() throws Exception {
    settings.setProperty(StyleCopConstants.MODE, StyleCopSensor.MODE_SKIP);
    initializeSensor();

    Project project = mock(Project.class);
    when(project.getLanguageKey()).thenReturn("cs");
    when(project.getName()).thenReturn("Project #1");

    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnTestProject() throws Exception {

    when(vsProject.isTest()).thenReturn(true);
    initializeSensor();

    Project project = mock(Project.class);
    when(project.getLanguageKey()).thenReturn("cs");
    when(project.getName()).thenReturn("Project #1");
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testUnitTestsSensorShouldNotExecuteOnRegularProject() throws Exception {
    initializeTestSensor();

    when(project.getLanguageKey()).thenReturn("cs");
    when(project.getName()).thenReturn("Project #1");

    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testGenerateConfigurationFile() throws Exception {
    File sonarDir = new File("target/sonar");
    sonarDir.mkdirs();
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSonarWorkingDirectory()).thenReturn(sonarDir);
    StyleCopProfileExporter.RegularStyleCopProfileExporter profileExporter = mock(StyleCopProfileExporter.RegularStyleCopProfileExporter.class);
    doAnswer(new Answer<Object>() {

      public Object answer(InvocationOnMock invocation) throws IOException {
        FileWriter writer = (FileWriter) invocation.getArguments()[1];
        writer.write("Hello");
        return null;
      }
    }).when(profileExporter).exportProfile((RulesProfile) anyObject(), (FileWriter) anyObject());
    StyleCopSensor sensor = new StyleCopSensor.RegularStyleCopSensor(fileSystem, null, profileExporter, null, conf, null);

    sensor.generateConfigurationFile(project);
    File report = new File(sonarDir, StyleCopConstants.STYLECOP_RULES_FILE);
    assertTrue(report.exists());
    report.delete();
  }

  @Test(expected = SonarException.class)
  public void testGenerateConfigurationFileWithUnexistingFolder() throws Exception {
    File sonarDir = new File("target/sonar/NON-EXISTING-FOLDER");
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSonarWorkingDirectory()).thenReturn(sonarDir);
    StyleCopProfileExporter.RegularStyleCopProfileExporter profileExporter = mock(StyleCopProfileExporter.RegularStyleCopProfileExporter.class);
    doAnswer(new Answer<Object>() {

      public Object answer(InvocationOnMock invocation) throws IOException {
        FileWriter writer = (FileWriter) invocation.getArguments()[1];
        writer.write("Hello");
        return null;
      }
    }).when(profileExporter).exportProfile((RulesProfile) anyObject(), (FileWriter) anyObject());
    StyleCopSensor sensor = new StyleCopSensor.RegularStyleCopSensor(fileSystem, null, profileExporter, null, conf, null);

    sensor.generateConfigurationFile(project);
  }

}
