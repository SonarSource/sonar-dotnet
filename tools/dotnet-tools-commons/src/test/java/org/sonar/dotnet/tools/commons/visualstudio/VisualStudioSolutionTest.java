/*
 * .NET tools :: Commons
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
package org.sonar.dotnet.tools.commons.visualstudio;

import static org.junit.Assert.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.util.List;

import org.junit.Test;
import org.sonar.api.resources.Project;
import org.sonar.test.TestUtils;


public class VisualStudioSolutionTest {
  
  private static final String SOLUTION_PATH = "/solution/Example/Example.sln";
  private static final String SILVERLIGHT_SOLUTION_PATH = "/solution/BlankSilverlightSolution/BlankSilverlightSolution.sln";
  private static final String WEB_SOLUTION_PATH = "/solution/web-solution/web-solution.sln";

  @Test
  public void testGetProjectFile() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    File sourceFile = TestUtils.getResource("/solution/Example/Example.Core/Money.cs");
    VisualStudioProject project = solution.getProject(sourceFile);
    assertEquals("Example.Core", project.getName());
  }
  
  @Test
  public void testGetProjectWithFileOutside() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    File sourceFile = TestUtils.getResource("/solution/LinkTestSolution/src/AssemblyInfo.cs");
    VisualStudioProject project = solution.getProject(sourceFile);
    assertNull(project);
  }
  
  @Test
  public void testGetProjectWithFakeFile() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    File sourceFile = TestUtils.getResource("/solution/Example/Example.Core/FooBar.cs");
    VisualStudioProject project = solution.getProject(sourceFile);
    assertNull(project);
  }
  
  @Test
  public void testGetTestProjects() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    List<VisualStudioProject> testProjects = solution.getTestProjects();
    assertEquals(1, testProjects.size());
  }
  
  @Test
  public void testGetTestProjectsNoTest() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SILVERLIGHT_SOLUTION_PATH));
    List<VisualStudioProject> testProjects = solution.getTestProjects();
    assertEquals(0, testProjects.size());
  }
  
  @Test
  public void testIsAspUsed() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    assertFalse(solution.isAspUsed());
  }
  
  @Test
  public void testIsAspUsedOk() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(WEB_SOLUTION_PATH));
    assertTrue(solution.isAspUsed());
  }
  
  @Test
  public void testIsSilverlightUsed() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    assertFalse(solution.isSilverlightUsed());
  }
  
  @Test
  public void testIsSilverlightUsedOk() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SILVERLIGHT_SOLUTION_PATH));
    assertTrue(solution.isSilverlightUsed());
  }
  
  @Test
  public void testGetProjectFromSonarProject() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Example.Application");
    VisualStudioProject vsProject = solution.getProjectFromSonarProject(project);
    assertEquals("Example.Application", vsProject.getName());
  }
  
  @Test
  public void testGetProjectFromSonarProjectWithBranch() throws Exception {
    VisualStudioSolution solution = ModelFactory.getSolution(TestUtils.getResource(SOLUTION_PATH));
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Example.Application MyBranch");
    when(project.getBranch()).thenReturn("MyBranch");
    VisualStudioProject vsProject = solution.getProjectFromSonarProject(project);
    assertEquals("Example.Application", vsProject.getName());
  }
}
