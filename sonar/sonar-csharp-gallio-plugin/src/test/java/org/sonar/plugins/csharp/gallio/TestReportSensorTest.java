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

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.util.Collections;

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
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;
import org.sonar.plugins.csharp.gallio.results.execution.GallioResultParser;
import org.sonar.plugins.csharp.gallio.results.execution.model.TestCaseDetail;
import org.sonar.plugins.csharp.gallio.results.execution.model.TestStatus;
import org.sonar.plugins.csharp.gallio.results.execution.model.UnitTestReport;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

@RunWith(PowerMockRunner.class)
@PrepareForTest(org.sonar.api.resources.File.class)
public class TestReportSensorTest {

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject1;
  private VisualStudioProject vsTestProject2;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private Project project;
  private GallioResultParser parser;
  
  private File fakeTestSourceFile = TestUtils.getResource("FakeTest.cs");

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
    when(project.getName()).thenReturn("Project Test 2");
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getTestDirs()).thenReturn(Collections.singletonList(TestUtils.getResource(".")));
    when(project.getFileSystem()).thenReturn(fileSystem);
    
    parser = mock(GallioResultParser.class); // create the parser before the sensor
  }
  
  @Test
  public void testAnalyseEmpty() {
    
    Configuration conf = new BaseConfiguration();
    TestReportSensor sensor = buildSensor(conf);
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/execution/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    when(parser.parse(any(File.class))).thenReturn(Collections.EMPTY_SET);

    SensorContext context = mock(SensorContext.class);
    
    sensor.analyse(project, context);
    
    verify(parser).parse(any(File.class));
  }
  
  @Test
  public void testAnalyse() {
    
    Configuration conf = new BaseConfiguration();
    TestReportSensor sensor = buildSensor(conf);
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/execution/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    UnitTestReport testReport = buildUnitTestReport(TestStatus.SUCCESS, null, null);
    
    File defaultReportFile = new File(solutionDir, "gallio-report.xml");
    
    when(parser.parse(eq(defaultReportFile))).thenReturn(Collections.singleton(testReport));

    SensorContext context = mock(SensorContext.class);
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(eq(fakeTestSourceFile), anyList())).thenReturn(sonarFile);
    
    
    sensor.analyse(project, context);
    
    verify(parser).parse(any(File.class));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TESTS), eq((double)1));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_EXECUTION_TIME), eq((double)42));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_ERRORS), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_FAILURES), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_SUCCESS_DENSITY), eq((double)100));
    
  }
  
  @Test
  public void testAnalyseWithError() {
    
    Configuration conf = new BaseConfiguration();
    TestReportSensor sensor = buildSensor(conf);
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/execution/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    UnitTestReport testReport = buildUnitTestReport(TestStatus.ERROR, "bad bad", "bad stack");
    
    when(parser.parse(any(File.class))).thenReturn(Collections.singleton(testReport));

    SensorContext context = mock(SensorContext.class);
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(eq(fakeTestSourceFile), anyList())).thenReturn(sonarFile);
    
    
    sensor.analyse(project, context);
    
    verify(parser).parse(any(File.class));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TESTS), eq((double)1));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_EXECUTION_TIME), eq((double)42));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_ERRORS), eq((double)1));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_FAILURES), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_SUCCESS_DENSITY), eq((double)0));
    
  }
  
  @Test
  public void testAnalyseUsingSafeMode() {
    
    Configuration conf = new BaseConfiguration();
    conf.setProperty(GallioConstants.SAFE_MODE, true);
    
    TestReportSensor sensor = buildSensor(conf);
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/execution/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    UnitTestReport testReport = buildUnitTestReport(TestStatus.SUCCESS, null, null);
    
    File reportFile = new File(solutionDir, "AssemblyTest2.gallio-report.xml");
    
    when(parser.parse(eq(reportFile))).thenReturn(Collections.singleton(testReport));

    SensorContext context = mock(SensorContext.class);
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(eq(fakeTestSourceFile), anyList())).thenReturn(sonarFile);
    
    
    sensor.analyse(project, context);
    
    verify(parser).parse(any(File.class));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TESTS), eq((double)1));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_EXECUTION_TIME), eq((double)42));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_ERRORS), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_FAILURES), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_SUCCESS_DENSITY), eq((double)100));
    
  }
  
  @Test
  public void testAnalyseWithReuseReport() {
    
    Configuration conf = new BaseConfiguration();
    conf.setProperty(GallioConstants.MODE, AbstractCSharpSensor.MODE_REUSE_REPORT);
    conf.setProperty(GallioConstants.REPORTS_PATH_KEY, "gallio-report.xml");
    
    TestReportSensor sensor = buildSensor(conf);
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/execution/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    UnitTestReport testReport = buildUnitTestReport(TestStatus.SUCCESS, null, null);
    
    File reportFile = new File(solutionDir, "gallio-report.xml");
    
    when(parser.parse(eq(reportFile))).thenReturn(Collections.singleton(testReport));

    SensorContext context = mock(SensorContext.class);
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(eq(fakeTestSourceFile), anyList())).thenReturn(sonarFile);
    
    
    sensor.analyse(project, context);
    
    verify(parser).parse(any(File.class));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TESTS), eq((double)1));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_EXECUTION_TIME), eq((double)42));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_ERRORS), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_FAILURES), eq((double)0));
    verify(context, atLeastOnce()).saveMeasure(eq(sonarFile), eq(CoreMetrics.TEST_SUCCESS_DENSITY), eq((double)100));
    
  }
  
  @Test
  public void testAnalyseWithReuseReportNotFound() {
    
    Configuration conf = new BaseConfiguration();
    conf.setProperty(GallioConstants.MODE, AbstractCSharpSensor.MODE_REUSE_REPORT);
    conf.setProperty(GallioConstants.REPORTS_PATH_KEY, "gallio-report-bad.xml");
    
    TestReportSensor sensor = buildSensor(conf);
    
    microsoftWindowsEnvironment.setTestExecutionDone();
    File solutionDir = TestUtils.getResource("/Results/execution/");
    microsoftWindowsEnvironment.setWorkingDirectory("");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getProject("MyAssembly")).thenReturn(vsProject1);
    
    UnitTestReport testReport = buildUnitTestReport(TestStatus.SUCCESS, null, null);
    
    File reportFile = new File(solutionDir, "gallio-report-bad.xml");
    
    when(parser.parse(eq(reportFile))).thenReturn(Collections.singleton(testReport));

    SensorContext context = mock(SensorContext.class);
    
    PowerMockito.mockStatic(org.sonar.api.resources.File.class);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    
    when(org.sonar.api.resources.File.fromIOFile(eq(fakeTestSourceFile), anyList())).thenReturn(sonarFile);
    
    
    sensor.analyse(project, context);
    
    verifyNoMoreInteractions(parser);
    verifyNoMoreInteractions(context);
  }
  
  private UnitTestReport buildUnitTestReport(TestStatus status, String errorMsg, String stack) {
    UnitTestReport testReport = new UnitTestReport();
    testReport.setAssemblyName("MyAssembly");
    testReport.setSourceFile(fakeTestSourceFile);
    TestCaseDetail detail = new TestCaseDetail();
    detail.setCountAsserts(32);
    detail.setTimeMillis(42);
    detail.setStatus(status);
    
    //only valid when status not succes
    detail.setErrorMessage(errorMsg);
    detail.setStackTrace(stack);
    
    testReport.addDetail(detail);
    return testReport;
  }

  @Test
  public void testShouldExecuteOnProject() throws Exception {
    Configuration conf = new BaseConfiguration();
    TestReportSensor sensor = buildSensor(conf);
    assertTrue(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfSkip() throws Exception {
    Configuration conf = new BaseConfiguration();
    conf.setProperty(GallioConstants.MODE, TestReportSensor.MODE_SKIP);
    TestReportSensor sensor = buildSensor(conf);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfNotTestProject() throws Exception {
    Configuration conf = new BaseConfiguration();
    TestReportSensor sensor = buildSensor(conf);
    when(project.getName()).thenReturn("Project #1");
    assertFalse(sensor.shouldExecuteOnProject(project));
  }
  
  private TestReportSensor buildSensor(Configuration conf) {
    return new TestReportSensor(new CSharpConfiguration(conf), microsoftWindowsEnvironment, parser);
  }
}