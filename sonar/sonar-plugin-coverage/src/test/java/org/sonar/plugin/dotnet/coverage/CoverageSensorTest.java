/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

package org.sonar.plugin.dotnet.coverage;

import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyDouble;
import static org.mockito.Mockito.*;

import net.sourceforge.pmd.rules.design.OnlyOneReturnRule;

import org.powermock.api.mockito.PowerMockito;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.configuration.Configuration;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;
import org.powermock.reflect.Whitebox;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.coverage.model.CoveragePoint;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ParserResult;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;
import org.sonar.plugin.dotnet.coverage.stax.CoverageResultStaxParser;

@RunWith(PowerMockRunner.class)
@PrepareForTest({CSharpFileLocator.class})
public class CoverageSensorTest {

  private CoveragePluginHandler pluginHandler;
  private CoverageResultStaxParser staxParser;
  private CoverageSensor sensor;

  @Before
  public void setUp() {
    pluginHandler = mock(CoveragePluginHandler.class);
    staxParser = mock(CoverageResultStaxParser.class);
    sensor = new CoverageSensor(pluginHandler, staxParser);
  }

  @Test
  public void testOnlyOneFile(){

    // set up maven project
    MavenProject mvnProject = createMvnProject();
    
    SensorContext context = mock(SensorContext.class);
    Project project = mock(Project.class);
    when(project.getPom()).thenReturn(mvnProject);
    Configuration configuration =  mock(Configuration.class);
    
    ProjectFileSystem projectFileSystem = mock(ProjectFileSystem.class);
    when(project.getFileSystem()).thenReturn(projectFileSystem);
    when(project.getConfiguration()).thenReturn(configuration);
    when(projectFileSystem.getBuildDir()).thenReturn(new File("target/test-classes/solution/MessyTestSolution/target"));
    when(projectFileSystem.getBasedir()).thenReturn(new File("target/test-classes/solution/MessyTestSolution"));
    
    when(staxParser.parse(eq(project), any(File.class))).thenReturn(buildParserResultWithOnlyOneFile());
    
    mockLocator(project);
    sensor.analyse(project, context);

    // 2 lines out of 3 are covered --> 2/3 line coverage
    verify(context, times(1)).saveMeasure(eq(CoreMetrics.COVERAGE), eq(67.0D));
  }

  @Test
  public void testTwoFiles(){

    // set up maven project
    MavenProject mvnProject = createMvnProject();
    
    SensorContext context = mock(SensorContext.class);
    Project project = mock(Project.class);
    when(project.getPom()).thenReturn(mvnProject);
    Configuration configuration =  mock(Configuration.class);
    
    ProjectFileSystem projectFileSystem = mock(ProjectFileSystem.class);
    when(project.getFileSystem()).thenReturn(projectFileSystem);
    when(project.getConfiguration()).thenReturn(configuration);
    when(projectFileSystem.getBuildDir()).thenReturn(new File("target/test-classes/solution/MessyTestSolution/target"));
    when(projectFileSystem.getBasedir()).thenReturn(new File("target/test-classes/solution/MessyTestSolution"));
    
    when(staxParser.parse(eq(project), any(File.class))).thenReturn(buildParserResultWithTwoFiles());
    
    mockLocator(project);
    
    sensor.analyse(project, context);

    // 5 lines out of 20 are covered --> 25% of line coverage
    verify(context, times(1)).saveMeasure(eq(CoreMetrics.COVERAGE), eq(25.0D)); 
  }
  
  private void mockLocator(Project project) {
    CSharpFileLocator locator = PowerMockito.mock(CSharpFileLocator.class); 
    PowerMockito.mockStatic(CSharpFileLocator.class);
    PowerMockito.when(CSharpFileLocator.values()).thenReturn(new CSharpFileLocator[] { locator }); 
    Whitebox.setInternalState(CSharpFileLocator.class, "INSTANCE", locator); 
    CSharpFile csFile= mock(CSharpFile.class);
    PowerMockito.when(locator.locate(eq(project), any(File.class), eq(false))).thenReturn(csFile);
  }
  
  private ParserResult buildParserResultWithOnlyOneFile(){
    List<ProjectCoverage> projects = new ArrayList<ProjectCoverage>();
    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>();
    
    File file = new File("MessyTestSolution/Money.cs");
    FileCoverage fileCoverage = new FileCoverage(file);
    fileCoverage.setAssemblyName("Money");
    sourceFiles.add(fileCoverage);

    fileCoverage.addPoint( createPoint(1, 1, 0) );
    fileCoverage.addPoint( createPoint(2, 2, 12) );
    fileCoverage.addPoint( createPoint(3, 3, 12) );
    fileCoverage.summarize();
    
    ProjectCoverage projectCoverage = new ProjectCoverage();
    projectCoverage.setAssemblyName("Money");
    projectCoverage.addFile(fileCoverage);
    projectCoverage.summarize();
    projects.add(projectCoverage);

    return new ParserResult(projects, sourceFiles);
  }
  
  private ParserResult buildParserResultWithTwoFiles(){
    List<ProjectCoverage> projects = new ArrayList<ProjectCoverage>();
    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>();
    
    File firstFile = new File("MessyTestSolution/Money.cs");
    FileCoverage firstFileCoverage = new FileCoverage(firstFile);
    firstFileCoverage.setAssemblyName("Money");
    sourceFiles.add(firstFileCoverage);

    // 10 lines
    // 10 not covered
    firstFileCoverage.addPoint( createPoint(1, 10, 0) );
    firstFileCoverage.summarize();
    
    ProjectCoverage firstProjectCoverage = new ProjectCoverage();
    firstProjectCoverage.setAssemblyName("Money");
    firstProjectCoverage.addFile(firstFileCoverage);
    firstProjectCoverage.summarize();
    projects.add(firstProjectCoverage);
    
    File secondFile = new File("MessyTestSolution/Program.cs");
    FileCoverage secondFileCoverage = new FileCoverage(secondFile);
    secondFileCoverage.setAssemblyName("Program");
    sourceFiles.add(secondFileCoverage);

    // 10 lines
    // 5 not covered
    secondFileCoverage.addPoint( createPoint(1, 1, 0) );
    secondFileCoverage.addPoint( createPoint(2, 2, 0) );
    secondFileCoverage.addPoint( createPoint(3, 3, 0) );
    secondFileCoverage.addPoint( createPoint(12, 12, 0) );
    secondFileCoverage.addPoint( createPoint(18, 22, 18) );
    secondFileCoverage.addPoint( createPoint(46, 46, 0) );
    secondFileCoverage.summarize();
    
    ProjectCoverage secondProjectCoverage = new ProjectCoverage();
    secondProjectCoverage.setAssemblyName("Program");
    secondProjectCoverage.addFile(secondFileCoverage);
    secondProjectCoverage.summarize();
    projects.add(secondProjectCoverage);
    
    return new ParserResult(projects, sourceFiles);
  }
  
  private MavenProject createMvnProject() {
    MavenProject mvnProject = new MavenProject();
    mvnProject.setPackaging("sln");
    mvnProject.getProperties().put(VisualStudioUtils.VISUAL_SOLUTION_NAME_PROPERTY, "MessyTestSolution.sln");
    File pomFile 
      = new File("target/test-classes/solution/MessyTestSolution/pom.xml");
    mvnProject.setFile(pomFile);
    return mvnProject;
  }
  
  private CoveragePoint createPoint(int startLine, int endLine, int countVisists){
    CoveragePoint point = new CoveragePoint();
    point.setStartLine(startLine);
    point.setEndLine(endLine);
    point.setCountVisits(countVisists);
    return point;
  }
}