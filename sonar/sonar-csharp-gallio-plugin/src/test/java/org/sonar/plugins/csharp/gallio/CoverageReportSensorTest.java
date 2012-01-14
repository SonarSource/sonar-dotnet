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
import org.junit.runner.RunWith;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;
import org.sonar.plugins.csharp.gallio.results.coverage.CoverageResultParser;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

@RunWith(PowerMockRunner.class)
@PrepareForTest(org.sonar.api.resources.File.class)
public class CoverageReportSensorTest {
  
  private VisualStudioSolution solution;
  private VisualStudioProject vsProject1;
  private VisualStudioProject vsTestProject2;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private Project project;
  private CoverageResultParser parser;
  private Configuration conf = new BaseConfiguration();
  private SensorContext context;
  private CoverageReportSensor sensor;
  private List<FileCoverage> sourceFiles;

  @Before
  public void init() {
    vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getName()).thenReturn("Project 1");
    vsTestProject2 = mock(VisualStudioProject.class);
    when(vsTestProject2.getName()).thenReturn("Project Test 2");
    when(vsTestProject2.getAssemblyName()).thenReturn("AssemblyTest2");
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
  
  private CoverageReportSensor buildSensor() {
    return new CoverageReportSensor(new CSharpConfiguration(conf), microsoftWindowsEnvironment, parser);
  }
  
  private void setUpEnv() {
    parser = mock(CoverageResultParser.class); // create the parser before the sensor
    sensor = buildSensor();
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/coverage/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    File defaultReportFile = new File(solutionDir, "coverage-report.xml"); 
    
    sourceFiles = new ArrayList<FileCoverage>();
    when(parser.parse(eq(project), eq(defaultReportFile))).thenReturn(sourceFiles);

    context = mock(SensorContext.class);
  }
  
  @Test
  public void testAnalyse() {
    setUpEnv();
    
    sensor.analyse(project, context);
   
  }
  
  
  @Test
  public void testAnalyseNotIndexedFile() {
    setUpEnv();

    FileCoverage fileCoverage = mock(FileCoverage.class);
    sourceFiles.add(fileCoverage);
    
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(any(File.class), eq(project))).thenReturn(sonarFile);
    
    SensorContext context = mock(SensorContext.class);
    when(context.isIndexed(eq(sonarFile), anyBoolean())).thenReturn(false);
    
    sensor.analyse(project, context);
   
    verify(context, only()).isIndexed(eq(sonarFile), anyBoolean());
  }
  
  @Test
  public void testAnalyseIndexedFile() {
    setUpEnv();

    FileCoverage fileCoverage = mock(FileCoverage.class);
    sourceFiles.add(fileCoverage);
    
    when(fileCoverage.getCoverage()).thenReturn(0.42);
    
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(any(File.class), eq(project))).thenReturn(sonarFile);
    
    SensorContext context = mock(SensorContext.class);
    when(context.isIndexed(eq(sonarFile), anyBoolean())).thenReturn(true);
    
    sensor.analyse(project, context);
   
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.COVERAGE), eq((double)42));
  }
  
  @Test
  public void testAnalyseIndexedFileInSafeMode() {
    parser = mock(CoverageResultParser.class); // create the parser before the sensor
    conf.setProperty(GallioConstants.SAFE_MODE, true);
    sensor = buildSensor();
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/coverage/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    List<FileCoverage> coverageList = new ArrayList<FileCoverage>();
    
    File defaultReportFile = new File(solutionDir, "AssemblyTest2.coverage-report.xml"); 
    when(parser.parse(eq(project), eq(defaultReportFile))).thenReturn(coverageList);

    context = mock(SensorContext.class);

    File fakeSourceFile = new File("dummy.cs");
    
    FileCoverage firstFileCoverage = mock(FileCoverage.class);
    when(firstFileCoverage.getCoverage()).thenReturn(0.15);
    when(firstFileCoverage.getFile()).thenReturn(fakeSourceFile);
    coverageList.add(firstFileCoverage);
    
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(any(File.class), eq(project))).thenReturn(sonarFile);
    
    SensorContext context = mock(SensorContext.class);
    when(context.isIndexed(eq(sonarFile), anyBoolean())).thenReturn(true);
    
    sensor.analyse(project, context);
   
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.COVERAGE), eq((double)15));  }
  
  @Test
  public void testReuseReportWithDefaultLocation() {
    parser = mock(CoverageResultParser.class); // create the parser before the sensor
    conf.setProperty(GallioConstants.MODE, AbstractCSharpSensor.MODE_REUSE_REPORT);
    sensor = buildSensor();
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/coverage/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    List<FileCoverage> coverageList = new ArrayList<FileCoverage>();
    
    File defaultReportFile = new File(solutionDir, "coverage-report.xml"); 
    when(parser.parse(eq(project), eq(defaultReportFile))).thenReturn(coverageList);

    context = mock(SensorContext.class);

    File fakeSourceFile = new File("dummy.cs");
    
    FileCoverage firstFileCoverage = mock(FileCoverage.class);
    when(firstFileCoverage.getCoverage()).thenReturn(0.15);
    when(firstFileCoverage.getFile()).thenReturn(fakeSourceFile);
    coverageList.add(firstFileCoverage);
    
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(any(File.class), eq(project))).thenReturn(sonarFile);
    
    SensorContext context = mock(SensorContext.class);
    when(context.isIndexed(eq(sonarFile), anyBoolean())).thenReturn(true);
    
    sensor.analyse(project, context);
   
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.COVERAGE), eq((double)15));
  }
  
  @Test
  public void testReuseTwoReports() {
    parser = mock(CoverageResultParser.class); // create the parser before the sensor
    conf.setProperty(GallioConstants.MODE, AbstractCSharpSensor.MODE_REUSE_REPORT);
    conf.setProperty(GallioConstants.REPORTS_COVERAGE_PATH_KEY, "report1.xml,report2.xml");
    sensor = buildSensor();
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/coverage/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    List<FileCoverage> firstCoverageList = new ArrayList<FileCoverage>();
    List<FileCoverage> secondCoverageList = new ArrayList<FileCoverage>();
    when(parser.parse(eq(project), any(File.class))).thenReturn(firstCoverageList, secondCoverageList);

    context = mock(SensorContext.class);

    File fakeSourceFile = new File("dummy.cs");
    File fakeSourceFile2 = new File("dummy2.cs");
    
    FileCoverage firstFileCoverage = mock(FileCoverage.class);
    when(firstFileCoverage.getCoverage()).thenReturn(0.15);
    when(firstFileCoverage.getFile()).thenReturn(fakeSourceFile);
    firstCoverageList.add(firstFileCoverage);
    
    FileCoverage secondFileCoverage = mock(FileCoverage.class);
    when(secondFileCoverage.getCoverage()).thenReturn(0.36);
    when(secondFileCoverage.getFile()).thenReturn(fakeSourceFile2);
    secondCoverageList.add(secondFileCoverage);

    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(any(File.class), eq(project))).thenReturn(sonarFile);
    
    SensorContext context = mock(SensorContext.class);
    when(context.isIndexed(eq(sonarFile), anyBoolean())).thenReturn(true);
    
    sensor.analyse(project, context);
   
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.COVERAGE), eq((double)15));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.COVERAGE), eq((double)36));
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
    conf.setProperty(GallioConstants.MODE, CoverageReportSensor.MODE_SKIP);
    CoverageReportSensor sensor = buildSensor();
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfTestProject() throws Exception {
    CoverageReportSensor sensor = buildSensor();
    when(project.getName()).thenReturn("Project Test 2");
    assertFalse(sensor.shouldExecuteOnProject(project));
  }
  
}