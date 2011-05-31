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

package org.sonar.plugin.dotnet.stylecop;

import static org.sonar.plugin.dotnet.stylecop.Constants.*;
import static org.mockito.Matchers.any;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.*;
import static org.junit.Assert.*;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.configuration.Configuration;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulesManager;
import org.sonar.api.rules.Violation;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.stylecop.stax.StyleCopResultStaxParser;

public class StyleCopSensorTest {

  private StyleCopSensor sensor;
  private RulesProfile profile;
  private RulesManager rulesManager;
  private StyleCopResultStaxParser styleCopResultStaxParser;
  private StyleCopPluginHandler pluginHandler;
  private Project project = mock(Project.class);
  private CSharpFileLocator fileLocator;

  @Before
  public void setUp() {
    pluginHandler = mock(StyleCopPluginHandler.class);
    profile = mock(RulesProfile.class);
    rulesManager = mock(RulesManager.class);
    styleCopResultStaxParser = mock(StyleCopResultStaxParser.class);
    when(styleCopResultStaxParser.parse(any(File.class))).thenReturn(buildStyleCopViolationResultParser());
    fileLocator = mock(CSharpFileLocator.class);

    MavenProject mvnProject = new MavenProject();
    mvnProject.setPackaging("sln");
    mvnProject.getProperties().put(VisualStudioUtils.VISUAL_SOLUTION_NAME_PROPERTY, "Example.sln");
    File pomFile 
    = new File("target/test-classes/solution/Example/pom.xml");
    mvnProject.setFile(pomFile);

    project = mock(Project.class);
    when(project.getPom()).thenReturn(mvnProject);

    Configuration configuration = mock(Configuration.class);
    when(project.getConfiguration()).thenReturn(configuration);
    when(project.getConfiguration().getString(anyString(), anyString())).thenReturn("reuseReport");

    ProjectFileSystem projectFileSystem = mock(ProjectFileSystem.class);

    when(projectFileSystem.getBuildDir()).thenReturn(new File("target/test-classes/solution/Example/target"));
    when(project.getFileSystem()).thenReturn(projectFileSystem);
    Resource res = mock(Resource.class);
    when(fileLocator.getResource(eq(project), anyString())).thenReturn(res);
    CSharpFile csFile = mock(CSharpFile.class);
    when(fileLocator.locate(eq(project), any(File.class), eq(false))).thenReturn(csFile);

    // set up rules manager
    Rule dummyRule = mock(Rule.class);
    when(rulesManager.getPluginRule(eq(StyleCopPlugin.KEY), anyString())).thenReturn(dummyRule);

    sensor = new StyleCopSensor(rulesManager, styleCopResultStaxParser, pluginHandler, profile, fileLocator);
  }

  @Test
  public void testShouldExecuteOnProject() {
    Project project = mock(Project.class);
    Configuration configuration =  mock(Configuration.class);
    when(project.getPackaging()).thenReturn("sln");
    when(project.getConfiguration()).thenReturn(configuration);
    assertTrue(sensor.shouldExecuteOnProject(project)); 
  }

  @Test
  public void testShouldExecuteOnProjectAndNoSlnProject() {
    Project project = mock(Project.class);
    Configuration configuration =  mock(Configuration.class);
    when(project.getPackaging()).thenReturn("pom");
    when(project.getConfiguration()).thenReturn(configuration);
    assertFalse(sensor.shouldExecuteOnProject(project)); 
  }

  @Test
  public void testShouldExecuteOnProjectWithSkip() {
    Configuration configuration =  mock(Configuration.class);
    when(configuration.getString(STYLECOP_MODE_KEY, STYLECOP_DEFAULT_MODE)).thenReturn(STYLECOP_SKIP_MODE);
    when(project.getPackaging()).thenReturn("sln");
    when(project.getConfiguration()).thenReturn(configuration);
    assertFalse(sensor.shouldExecuteOnProject(project)); 
  }

  private void testAnalyseReuse(String reportPathParam) {

    SensorContext context = mock(SensorContext.class);
    when(project.getConfiguration().getString(STYLECOP_REPORT_KEY)).thenReturn(reportPathParam);

    sensor.analyse(project, context);

    verify(context,atLeast(2)).saveViolation(any(Violation.class));
  }

  @Test
  public void testAnalyseReuseSameDir() {
    testAnalyseReuse("stylecop-alt-report.xml");
  }

  @Test
  public void testAnalyseReuseDifferentDir() {
    testAnalyseReuse("../working/stylecop-report.xml");
  }

  private List<StyleCopViolation> buildStyleCopViolationResultParser(){
    StyleCopViolation dummyViolation = new StyleCopViolation("9", "dummyPath.cs", "dummyKey", "This is a dummyMessage");
    StyleCopViolation anOtherDummyViolation = new StyleCopViolation("12", "anOtherDummyPath.cs", "anOtherDummyKey", "This is an other dummyMessage");
    List<StyleCopViolation> dummyViolations = new ArrayList<StyleCopViolation>();
    dummyViolations.add(dummyViolation);
    dummyViolations.add(anOtherDummyViolation);
    return dummyViolations;
  }



}
