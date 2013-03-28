/*
 * Sonar .NET Plugin :: Gallio
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
package org.sonar.plugins.csharp.gallio;

import com.google.common.collect.Lists;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.ArgumentCaptor;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.config.PropertyDefinitions;
import org.sonar.api.config.Settings;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.gallio.GallioCommandBuilder;
import org.sonar.dotnet.tools.gallio.GallioRunner;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.DotNetConstants;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;
import org.sonar.plugins.dotnet.core.DotNetCorePlugin;
import org.sonar.test.TestUtils;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.anyBoolean;
import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

@RunWith(PowerMockRunner.class)
@PrepareForTest(GallioRunner.class)
public class GallioSensorTest {

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject1;
  private VisualStudioProject vsTestProject2;
  private VisualStudioProject vsTestProject3;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private Project project;
  private Settings conf;

  @Before
  public void init() {
    vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getName()).thenReturn("Project #1");
    when(vsProject1.getArtifact("Debug", "Any CPU")).thenReturn(
        TestUtils.getResource("/Sensor/FakeAssemblies/Fake1.assembly")
        );

    vsTestProject2 = mock(VisualStudioProject.class);
    when(vsTestProject2.getName()).thenReturn("Project Test #2");
    when(vsTestProject2.getArtifact("Debug", "Any CPU")).thenReturn(
        TestUtils.getResource("/Sensor/FakeAssemblies/Fake2.assembly")
        );
    when(vsTestProject2.isTest()).thenReturn(true);

    vsTestProject3 = mock(VisualStudioProject.class);
    when(vsTestProject3.getName()).thenReturn("Project Test #3");
    when(vsTestProject3.getArtifact("Debug", "Any CPU")).thenReturn(
        TestUtils.getResource("/Sensor/FakeAssemblies/Fake3.assembly")
        );
    when(vsTestProject3.isTest()).thenReturn(true);

    solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject1, vsTestProject2, vsTestProject3));
    when(solution.getUnitTestProjects()).thenReturn(Lists.newArrayList(vsTestProject2, vsTestProject3));

    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);

    project = mock(Project.class);
    when(project.getLanguageKey()).thenReturn("cs");
    when(project.getName()).thenReturn("Project #1");

    conf = new Settings(new PropertyDefinitions(new DotNetCorePlugin(), new GallioPlugin()));
  }

  @Test
  public void testAnalyse() throws Exception {
    SensorContext context = mock(SensorContext.class);
    microsoftWindowsEnvironment.setWorkingDirectory("coverage");
    File solutionDir = TestUtils.getResource("/Results/");
    when(solution.getSolutionDir()).thenReturn(solutionDir);

    GallioRunner runner = mock(GallioRunner.class);
    GallioCommandBuilder builder = mock(GallioCommandBuilder.class);
    when(runner.createCommandBuilder(solution)).thenReturn(builder);

    PowerMockito.mockStatic(GallioRunner.class);
    when(GallioRunner.create(anyString(), anyString(), anyBoolean())).thenReturn(runner);

    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    sensor.analyse(project, context);

    // One call with two assemblies
    ArgumentCaptor<List> testAssembliesCaptor = ArgumentCaptor.forClass(List.class);
    verify(builder).setTestAssemblies(testAssembliesCaptor.capture());
    assertEquals(2, testAssembliesCaptor.getValue().size());
  }

  @Test
  public void testAnalyseSafeMode() throws Exception {
    SensorContext context = mock(SensorContext.class);
    microsoftWindowsEnvironment.setWorkingDirectory("coverage");
    File solutionDir = TestUtils.getResource("/Results/");
    when(solution.getSolutionDir()).thenReturn(solutionDir);

    GallioRunner runner = mock(GallioRunner.class);
    GallioCommandBuilder builder = mock(GallioCommandBuilder.class);
    when(runner.createCommandBuilder(solution)).thenReturn(builder);

    PowerMockito.mockStatic(GallioRunner.class);
    when(GallioRunner.create(anyString(), anyString(), anyBoolean())).thenReturn(runner);

    // safe mode activation
    conf.setProperty(GallioConstants.SAFE_MODE_KEY, "true");

    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    sensor.analyse(project, context);

    // Two calls with a single assembly each time
    ArgumentCaptor<List> testAssembliesCaptor = ArgumentCaptor.forClass(List.class);
    verify(builder, times(2)).setTestAssemblies(testAssembliesCaptor.capture());
    List<List> assemblyLists = testAssembliesCaptor.getAllValues();
    for (List list : assemblyLists) {
      assertEquals(1, list.size());
    }
  }

  @Test
  public void testAnalyseWithPattern() throws Exception {
    SensorContext context = mock(SensorContext.class);
    microsoftWindowsEnvironment.setWorkingDirectory("coverage");
    File solutionDir = TestUtils.getResource("/Results/");
    when(solution.getSolutionDir()).thenReturn(solutionDir);

    GallioRunner runner = mock(GallioRunner.class);
    GallioCommandBuilder builder = mock(GallioCommandBuilder.class);
    when(runner.createCommandBuilder(solution)).thenReturn(builder);

    PowerMockito.mockStatic(GallioRunner.class);
    when(GallioRunner.create(anyString(), anyString(), anyBoolean())).thenReturn(runner);

    // pattern used to find test assemblies
    conf.setProperty(DotNetConstants.TEST_ASSEMBLIES_KEY, "$(SolutionDir)/../**/Fake3.*");

    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    sensor.analyse(project, context);

    // One call with only one assembly
    ArgumentCaptor<List> testAssembliesCaptor = ArgumentCaptor.forClass(List.class);
    verify(builder).setTestAssemblies(testAssembliesCaptor.capture());
    assertEquals(1, testAssembliesCaptor.getValue().size());
  }

  @Test
  public void testAnalyseWithPatternAndSafeMode() throws Exception {
    SensorContext context = mock(SensorContext.class);
    microsoftWindowsEnvironment.setWorkingDirectory("coverage");
    File solutionDir = TestUtils.getResource("/Results/");
    when(solution.getSolutionDir()).thenReturn(solutionDir);

    GallioRunner runner = mock(GallioRunner.class);
    GallioCommandBuilder builder = mock(GallioCommandBuilder.class);
    when(runner.createCommandBuilder(solution)).thenReturn(builder);

    PowerMockito.mockStatic(GallioRunner.class);
    when(GallioRunner.create(anyString(), anyString(), anyBoolean())).thenReturn(runner);

    // safe mode activation
    conf.setProperty(GallioConstants.SAFE_MODE_KEY, "true");
    // pattern used to find test assemblies
    conf.setProperty(DotNetConstants.TEST_ASSEMBLIES_KEY, "$(SolutionDir)/../**/Fake*.assembly");

    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    sensor.analyse(project, context);

    // Three calls with a single assembly each time
    ArgumentCaptor<List> testAssembliesCaptor = ArgumentCaptor.forClass(List.class);
    verify(builder, times(3)).setTestAssemblies(testAssembliesCaptor.capture());
    List<List> assemblyLists = testAssembliesCaptor.getAllValues();
    for (List list : assemblyLists) {
      assertEquals(1, list.size());
    }
  }

  @Test
  public void testShouldExecuteOnProject() throws Exception {
    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    assertTrue(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfSkip() throws Exception {
    conf.setProperty(GallioConstants.MODE, GallioSensor.MODE_SKIP);
    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfReuseReports() throws Exception {
    conf.setProperty(GallioConstants.MODE, GallioSensor.MODE_REUSE_REPORT);
    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectTestsAlreadyExecuted() throws Exception {
    microsoftWindowsEnvironment.setTestExecutionDone();
    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfNoTests() throws Exception {
    when(solution.getUnitTestProjects()).thenReturn(new ArrayList<VisualStudioProject>());
    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnNotCSharpProject() throws Exception {
    // Non C# project will have an empty MicrosoftWindowsEnvironement with no solution
    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    when(project.getLanguageKey()).thenReturn("fortran");
    GallioSensor sensor = new GallioSensor(new DotNetConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }
}
