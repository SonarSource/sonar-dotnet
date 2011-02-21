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

package org.sonar.plugin.dotnet.core.project;

import static org.junit.Assert.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.util.Map;

import org.apache.commons.configuration.Configuration;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.resources.Project;

public class VisualUtilsTest {

  private Project project;
  private Configuration configuration;
  
  @Before
  public void configureProject() {
    // set up maven project
    MavenProject mvnProject = new MavenProject();
    mvnProject.setPackaging("sln");
    mvnProject.getProperties().put(VisualStudioUtils.VISUAL_SOLUTION_NAME_PROPERTY, "Example.sln");
    File pomFile 
      = new File("target/test-classes/solution/Example/pom.xml");
    mvnProject.setFile(pomFile);
    
    // set up sonar project
    project = mock(Project.class);
    configuration = mock(Configuration.class);
    when(project.getPom()).thenReturn(mvnProject);
    when(project.getConfiguration()).thenReturn(configuration);
  }
  
  
  @Test
  public void testBuildCsFileProjectMap() {
    Map<File, VisualStudioProject> csFileMap = VisualUtils.buildCsFileProjectMap(project);
    assertFalse(csFileMap.keySet().isEmpty());
    
    assertEquals(10, csFileMap.keySet().size());
  }
  
  @Test
  public void testBuildCsFileProjectMapWithExclusion() {
    // filter one cs file
    when(project.getExclusionPatterns()).thenReturn(new String[]{"**/Model/**/*.cs"});
    
    Map<File, VisualStudioProject> csFileMap = VisualUtils.buildCsFileProjectMap(project);
    assertFalse(csFileMap.keySet().isEmpty());
    assertEquals(9, csFileMap.keySet().size());
    
  }
  
  @Test
  public void testBuildCsFileProjectMapWithOneModuleExcluded() {
    // filter one module
    when(configuration.getStringArray("sonar.skippedModules")).thenReturn(new String[]{"Example.Application"});
    
    Map<File, VisualStudioProject> csFileMap = VisualUtils.buildCsFileProjectMap(project);
    assertFalse(csFileMap.keySet().isEmpty());
    assertEquals(8, csFileMap.keySet().size());
  }
  
  @Test
  public void testBuildCsFileProjectMapWithAllModulesExcluded() {
    // filter one module
    when(configuration.getStringArray("sonar.skippedModules")).thenReturn(new String[]{"Example.Application", "Example.Core", "Example.Core.Tests"});
    
    Map<File, VisualStudioProject> csFileMap = VisualUtils.buildCsFileProjectMap(project);
    assertTrue(csFileMap.keySet().isEmpty());
  }

}
