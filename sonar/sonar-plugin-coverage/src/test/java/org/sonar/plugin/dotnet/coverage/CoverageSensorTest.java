/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.coverage;

import static org.mockito.Matchers.*;
import static org.mockito.AdditionalMatchers.*;


import static org.mockito.Mockito.*;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.configuration.Configuration;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.coverage.model.CoveragePoint;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ParserResult;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;
import org.sonar.plugin.dotnet.coverage.stax.CoverageResultStaxParser;

public class CoverageSensorTest {

  private CoveragePluginHandler pluginHandler;
  private CoverageResultStaxParser staxParser;
  private CoverageSensor sensor;
  private CSharpFileLocator fileLocator;
  private Project project;

  @Before
  public void setUp() {
    pluginHandler = mock(CoveragePluginHandler.class);
    staxParser = mock(CoverageResultStaxParser.class);
    fileLocator = mock(CSharpFileLocator.class);
    sensor = new CoverageSensor(pluginHandler, staxParser, fileLocator);
    
    // set up maven project
    MavenProject mvnProject = createMvnProject();
    project = mock(Project.class);
    when(project.getPom()).thenReturn(mvnProject);
    Configuration configuration =  mock(Configuration.class);
    
    ProjectFileSystem projectFileSystem = mock(ProjectFileSystem.class);
    when(project.getFileSystem()).thenReturn(projectFileSystem);
    when(project.getConfiguration()).thenReturn(configuration);
    when(projectFileSystem.getBuildDir()).thenReturn(new File("target/test-classes/solution/MessyTestSolution/target"));
    when(projectFileSystem.getBasedir()).thenReturn(new File("target/test-classes/solution/MessyTestSolution"));
    
    
    CSharpFile csFile= mock(CSharpFile.class);
    when(fileLocator.locate(eq(project), any(File.class), eq(false))).thenReturn(csFile);
  }

  @Test
  public void testOnlyOneFile(){

    SensorContext context = mock(SensorContext.class);
    when(staxParser.parse(eq(project), any(File.class))).thenReturn(buildParserResultWithOnlyOneFile());
    
    sensor.analyse(project, context);

    // 2 lines out of 3 are covered --> 2/3 line coverage
    verify(context, times(1)).saveMeasure(eq(CoreMetrics.COVERAGE), eq(67.0D));
    // 3 times because of 3 levels : file, folder, assembly
    verify(context, times(3)).saveMeasure(any(Resource.class), eq(CoreMetrics.COVERAGE), eq(67.0D));
  }

  
  private ParserResult buildParserResultWithOnlyOneFile(){
    List<ProjectCoverage> projects = new ArrayList<ProjectCoverage>();
    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>();
    
    File file = new File("target/test-classes/solution/MessyTestSolution/Money.cs");
    FileCoverage fileCoverage = new FileCoverage(file);
    fileCoverage.setAssemblyName("MessyTestApplication");
    sourceFiles.add(fileCoverage);

    fileCoverage.addPoint( createPoint(1, 1, 0) );
    fileCoverage.addPoint( createPoint(2, 2, 12) );
    fileCoverage.addPoint( createPoint(3, 3, 12) );
    fileCoverage.summarize();
    
    ProjectCoverage projectCoverage = new ProjectCoverage();
    projectCoverage.setAssemblyName("MessyTestApplication");
    projectCoverage.addFile(fileCoverage);
    projectCoverage.summarize();
    projects.add(projectCoverage);

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