/*
 * Sonar C# Plugin :: Gallio
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

import static org.junit.Assert.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.gallio.results.coverage.CoverageResultParser;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ParserResult;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ProjectCoverage;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class CoverageReportSensorTest {

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject1;
  private VisualStudioProject vsTestProject2;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private Project project;
  private CoverageResultParser parser;
  private Configuration conf = new BaseConfiguration();

  @Before
  public void init() {
    vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getName()).thenReturn("Project #1");
    vsTestProject2 = mock(VisualStudioProject.class);
    when(vsTestProject2.getName()).thenReturn("Project Test #2");
    when(vsTestProject2.isTest()).thenReturn(true);
    solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject1, vsTestProject2));
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsTestProject2));

    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);

    project = mock(Project.class);
    when(project.getLanguageKey()).thenReturn("cs");
    when(project.getName()).thenReturn("Project #1");
  }
  
  @Test
  public void testAnalyse() {
    parser = mock(CoverageResultParser.class); // create the parser before the sensor
    CoverageReportSensor sensor = buildSensor();
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    microsoftWindowsEnvironment.setWorkingDirectory(".");
    File solutionDir = TestUtils.getResource("/Results/coverage/");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>();
    List<ProjectCoverage> projects = new ArrayList<ProjectCoverage>();
    ParserResult parserResult = new ParserResult(projects, sourceFiles);
    when(parser.parse(eq(project), any(File.class))).thenReturn(parserResult);

    
    ProjectCoverage projectCoverage = mock(ProjectCoverage.class);
    when(projectCoverage.getAssemblyName()).thenReturn("MyAssembly");
    projects.add(projectCoverage);
    
    SensorContext context = mock(SensorContext.class);
    
    sensor.analyse(project, context);
    
    verify(projectCoverage).getFileCoverageCollection();
  }
  
  @Test
  public void testAnalyseWithBranch() {
    when(project.getBranch()).thenReturn("ProductionWhatever");
    when(project.getName()).thenReturn("Project #1 ProductionWhatever");
    testAnalyse();
  }
  

  @Test
  public void testShouldExecuteOnProject() throws Exception {
    CoverageReportSensor sensor = buildSensor();
    assertTrue(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfSkip() throws Exception {
    conf.setProperty(GallioConstants.MODE, GallioConstants.MODE_SKIP);
    CoverageReportSensor sensor = buildSensor();
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfTestProject() throws Exception {
    CoverageReportSensor sensor = buildSensor();
    when(project.getName()).thenReturn("Project Test #2");
    assertFalse(sensor.shouldExecuteOnProject(project));
  }
  
  private CoverageReportSensor buildSensor() {
    return new CoverageReportSensor(new CSharpConfiguration(conf), microsoftWindowsEnvironment, parser);
  }
  
}