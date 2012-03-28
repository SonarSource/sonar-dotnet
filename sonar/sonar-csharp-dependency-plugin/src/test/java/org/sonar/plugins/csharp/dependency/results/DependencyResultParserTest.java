/*
 * Sonar C# Plugin :: Dependency
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

package org.sonar.plugins.csharp.dependency.results;

import static org.junit.Assert.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.util.List;

import org.junit.Before;
import org.junit.Test;
import org.mockito.ArgumentCaptor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Library;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.test.TestUtils;


public class DependencyResultParserTest {

  private DependencyResultParser parser;
  private CSharpResourcesBridge bridge;
  private SensorContext context;
  private Project project;
  
  @Before
  public void setUp() {
    bridge = mock(CSharpResourcesBridge.class);
    context = mock(SensorContext.class);
    
    project = mock(Project.class);
    Project rootProject = mock(Project.class);
    when(project.getParent()).thenReturn(rootProject);
    
    VisualStudioSolution vsSolution = mock(VisualStudioSolution.class);
    VisualStudioProject vsProject = mock(VisualStudioProject.class);
    when(vsSolution.getProject("Example.Core")).thenReturn(vsProject);
    when(vsSolution.getProjectFromSonarProject(project)).thenReturn(vsProject);
    
    MicrosoftWindowsEnvironment env = mock(MicrosoftWindowsEnvironment.class);
    when(env.getCurrentSolution()).thenReturn(vsSolution);
    parser = new DependencyResultParser(env , bridge, project, context);
  }
  
  @Test
  public void testParse() {
    File report = TestUtils.getResource("/dependencyparser-report.xml");
    
    
    
    parser.parse("compile", report);
    
    ArgumentCaptor<Library> libCaptor = ArgumentCaptor.forClass(Library.class);
    verify(context, times(1)).getResource(libCaptor.capture());
    Library library = libCaptor.getValue();
    assertEquals("mscorlib", library.getName());
    
    ArgumentCaptor<String> classCaptor = ArgumentCaptor.forClass(String.class);
    verify(bridge, atLeast(3)).getFromTypeName(classCaptor.capture());
    List<String> classNames = classCaptor.getAllValues();
    
    assertTrue(classNames.contains("Example.Core.IMoney"));
    assertTrue(classNames.contains("Example.Core.Money"));
    assertTrue(classNames.contains("Example.Core.MoneyBag"));
    
    // Classes without dependencies
    assertFalse(classNames.contains("Example.Core.Dummy"));
    assertFalse(classNames.contains("Example.Core.SampleMeasure/Possible"));
    assertFalse(classNames.contains("Example.Core.SampleMeasure"));
    assertFalse(classNames.contains("Example.Core.Model.SubType"));
  }

}
