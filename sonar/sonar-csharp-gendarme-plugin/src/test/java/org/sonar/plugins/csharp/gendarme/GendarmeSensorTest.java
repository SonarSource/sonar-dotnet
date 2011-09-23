/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme;

import static org.hamcrest.Matchers.hasItems;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Matchers.anyString;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.doAnswer;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.rules.ActiveRule;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.dotnet.tools.gendarme.GendarmeCommandBuilder;
import org.sonar.dotnet.tools.gendarme.GendarmeRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.gendarme.profiles.GendarmeProfileExporter;
import org.sonar.plugins.csharp.gendarme.results.GendarmeResultParser;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;
import com.google.common.collect.Sets;

public class GendarmeSensorTest {

  private ProjectFileSystem fileSystem;
  private VisualStudioSolution solution;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  
  private VisualStudioProject vsProject;

  @Before
  public void init() throws Exception {
    fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSonarWorkingDirectory()).thenReturn(TestUtils.getResource("/Sensor"));

    vsProject = mock(VisualStudioProject.class);
    when(vsProject.getName()).thenReturn("Project #1");
    when(vsProject.getGeneratedAssemblies("Debug")).thenReturn(
        Sets.newHashSet(TestUtils.getResource("/Sensor/FakeAssemblies/Fake1.assembly")));
    solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject));

    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);
  }

  @Test
  public void testExecuteWithoutRule() throws Exception {
    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository(anyString())).thenReturn(new ArrayList<ActiveRule>());
    GendarmeSensor sensor = new GendarmeSensor(null, rulesProfile, null, null, new CSharpConfiguration(new BaseConfiguration()),
        microsoftWindowsEnvironment);

    Project project = mock(Project.class);
    sensor.analyse(project, null);
    verify(project, never()).getName();
  }

  @Test
  public void testLaunchGendarme() throws Exception {
    GendarmeSensor sensor = new GendarmeSensor(fileSystem, null, null, null, new CSharpConfiguration(new BaseConfiguration()),
        microsoftWindowsEnvironment);

    GendarmeRunner runner = mock(GendarmeRunner.class);
    GendarmeCommandBuilder builder = GendarmeCommandBuilder.createBuilder(solution, vsProject);
    builder.setExecutable(new File("gendarme.exe"));
    when(runner.createCommandBuilder(solution, vsProject)).thenReturn(builder);
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Project #1");

    sensor.launchGendarme(project, runner, TestUtils.getResource("/Sensor/FakeGendarmeConfigFile.xml"));
    verify(runner).execute(any(GendarmeCommandBuilder.class), eq(10));
  }

  @Test
  public void testShouldExecuteOnProject() throws Exception {
    Configuration conf = new BaseConfiguration();
    GendarmeSensor sensor = new GendarmeSensor(null, null, null, null, new CSharpConfiguration(conf), microsoftWindowsEnvironment);

    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Project #1");
    when(project.getLanguageKey()).thenReturn("cs");
    assertTrue(sensor.shouldExecuteOnProject(project));

    conf.addProperty(GendarmeConstants.MODE, GendarmeConstants.MODE_SKIP);
    sensor = new GendarmeSensor(null, null, null, null, new CSharpConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testGenerateConfigurationFile() throws Exception {
    File sonarDir = new File("target/sonar");
    sonarDir.mkdirs();
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSonarWorkingDirectory()).thenReturn(sonarDir);
    GendarmeProfileExporter profileExporter = mock(GendarmeProfileExporter.class);
    doAnswer(new Answer<Object>() {

      public Object answer(InvocationOnMock invocation) throws IOException {
        FileWriter writer = (FileWriter) invocation.getArguments()[1];
        writer.write("Hello");
        return null;
      }
    }).when(profileExporter).exportProfile((RulesProfile) anyObject(), (FileWriter) anyObject());
    GendarmeSensor sensor = new GendarmeSensor(fileSystem, null, profileExporter, null, new CSharpConfiguration(new BaseConfiguration()),
        microsoftWindowsEnvironment);

    sensor.generateConfigurationFile();
    File report = new File(sonarDir, GendarmeConstants.GENDARME_RULES_FILE);
    assertTrue(report.exists());
    report.delete();
  }

}
