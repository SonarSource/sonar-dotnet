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
package org.sonar.dotnet.tools.commons.utils;

import static org.junit.Assert.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.util.Collection;

import org.junit.Before;
import org.junit.Test;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;


public class FileFinderTest {

  private VisualStudioSolution solution;
  private VisualStudioProject project;
  
  @Before
  public void setUp() {
    solution = mock(VisualStudioSolution.class);
    project = mock(VisualStudioProject.class);
    File solutionDir = TestUtils.getResource("/solution/Example");
    File projectDir = TestUtils.getResource("/solution/Example/Example.Core");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(project.getDirectory()).thenReturn(projectDir);
    
  }
  
  @Test
  public void testFindFiles() {
    Collection<File> result = FileFinder.findFiles(solution, project, "Model\\SubType.cs");
    assertEquals(1, result.size());
    File csFile = result.iterator().next();
    assertTrue(csFile.exists());
    assertEquals("SubType.cs", csFile.getName());
  }
  
  @Test
  public void testFindSubDirectories() {
    Collection<File> result = FileFinder.findDirectories(solution, project, "**/*");
    assertFalse(result.isEmpty());
    for (File file : result) {
      assertTrue(file.exists());
      assertTrue(file.isDirectory());
    }
  }
  
  @Test
  public void testFindSubDirectoriesWithNull() {
    Collection<File> result = FileFinder.findDirectories(solution, project, (String[])null);
    assertNotNull(result);
    assertTrue(result.isEmpty());
  }
  
  @Test
  public void testFindAbsoluteFile() {
    File expectedCsFile = TestUtils.getResource("/solution/MessyTestSolution/MessyTestApplication/Program.cs");
    Collection<File> result = FileFinder.findFiles(solution, project, expectedCsFile.getAbsolutePath());
    assertEquals(1, result.size());
    File csFile = result.iterator().next();
    assertTrue(csFile.exists());
    assertEquals("Program.cs", csFile.getName());
  }
  
  @Test
  public void testFindAbsoluteFileWithPatternOutsideSolution() {
    File outsideDir = TestUtils.getResource("/solution/BlankSilverlightSolution");
    String pattern = outsideDir.getAbsolutePath() + "/**/*Info.cs";
    Collection<File> result = FileFinder.findFiles(solution, project, pattern);
    assertEquals(2, result.size());
    File csFile = result.iterator().next();
    assertTrue(csFile.exists());
    assertEquals("AssemblyInfo.cs", csFile.getName());
  }
  
  @Test
  public void testFindRelativeFileWithPatternOutsideSolution() {
    String pattern = "../../BlankSilverlightSolution/**/*Info.cs";
    Collection<File> result = FileFinder.findFiles(solution, project, pattern);
    assertEquals(2, result.size());
    File csFile = result.iterator().next();
    assertTrue(csFile.exists());
    assertEquals("AssemblyInfo.cs", csFile.getName());
  }
  
  @Test
  public void testFindFilesWithBadPattern() {
    Collection<File> result = FileFinder.findFiles(solution, project, "../toto/**/*.cs");
    assertNotNull(result);
    assertTrue(result.isEmpty());
  }
  
  @Test
  public void testFindFilesWithPattern() {
    Collection<File> result = FileFinder.findFiles(solution, project, "**/*.cs");
    assertEquals(6, result.size());
    for (File file : result) {
      assertTrue(file.exists());
    }
  }
  
  @Test
  public void testFindFilesWithMultiplePatterns() {
    Collection<File> result = FileFinder.findFiles(solution, project, "S*.cs;**/*Money.cs");
    assertEquals(3, result.size());
    for (File file : result) {
      assertTrue(file.exists());
    }
  }
  
  @Test
  public void testPatternWithSubstitution() {
    Collection<File> result = FileFinder.findFiles(solution, project, "$(SolutionDir)/Example.Core/*.cs");
    assertEquals(4, result.size());
    for (File file : result) {
      assertTrue(file.exists());
    }
  }
  
  @Test
  public void testPatternWithSubstitutionOutsideSolution() {
    Collection<File> result = 
      FileFinder.findFiles(solution, project, "$(SolutionDir)/../BlankSilverlightSolution/**/*Info.cs");
    assertEquals(2, result.size());
    File csFile = result.iterator().next();
    assertTrue(csFile.exists());
    assertEquals("AssemblyInfo.cs", csFile.getName());
  }

}
