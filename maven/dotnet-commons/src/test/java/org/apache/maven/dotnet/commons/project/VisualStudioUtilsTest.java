/**
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

/*
 * Created on Sep 1, 2009
 *
 */
package org.apache.maven.dotnet.commons.project;

import java.io.File;
import java.util.Collection;
import java.util.List;
import java.util.Set;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import static org.junit.Assert.*;
import org.junit.Test;
import static org.hamcrest.CoreMatchers.*;
import static org.junit.matchers.JUnitMatchers.*;
import org.slf4j.LoggerFactory;

/**
 * Tests for visual studio utilities.
 * 
 * @author Jose CHILLAN Sep 1, 2009
 */
public class VisualStudioUtilsTest {
  private final static org.slf4j.Logger log = LoggerFactory
      .getLogger(VisualStudioUtilsTest.class);
  /**
   * PROJECT_CORE_PATH
   */
  private static final String PROJECT_CORE_PATH = "target/test-classes/solution/Example/Example.Core/Example.Core.csproj";
  private static final String SAMPLE_FILE_PATH = "target/test-classes/solution/Example/Example.Core/Model/SubType.cs";
  private static final String SOLUTION_PATH = "target/test-classes/solution/Example/Example.sln";
  private static final String MESSY_SOLUTION_PATH = "target/test-classes/solution/MessyTestSolution/MessyTestSolution.sln";
  private static final String LINK_SOLUTION_PATH = "target/test-classes/solution/LinkTestSolution/LinkTestSolution.sln";
  private static final String SOLUTION_WITH_DUP_PATH = "target/test-classes/solution/DuplicatesExample/Example.sln";

  private static final String SILVERLIGHT_SOLUTION_PATH = "target/test-classes/solution/BlankSilverlightSolution/BlankSilverlightSolution.sln";
  private static final String SILVERLIGHT_PROJECT_PATH = "target/test-classes/solution/BlankSilverlightSolution/BlankApplication/BlankApplication.csproj";

  private static final String WEB_SOLUTION_PATH = "target/test-classes/solution/web-solution/web-solution.sln";
  
  @Test
  public void testReadFiles() {
    File file = new File(PROJECT_CORE_PATH);
    List<String> files = VisualStudioUtils.getFilesPath(file);
    log.debug("Files : " + files);
    assertEquals("Bad number of files extracted", 6, files.size());
  }

  @Test
  public void testReadSolution() throws Exception {
    File file = new File(SOLUTION_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    VisualStudioProject project = solution.getProject("Example.Core");
    Collection<SourceFile> files = project.getSourceFiles();
    assertEquals("Bad number of files extracted", 6, files.size());
    List<BinaryReference> references = project.getBinaryReferences();
    assertTrue(references.size()>0);
    
    BinaryReference systemReference = new BinaryReference();
    systemReference.setAssemblyName("System.Xml.Linq");
    systemReference.setVersion("v3.5");
    
    assertThat(references, hasItems(systemReference));
    
    VisualStudioProject testProject = solution.getProject("Example.Core.Tests");
    List<BinaryReference> testReferences = testProject.getBinaryReferences();
    BinaryReference nunitReference = new BinaryReference();
    nunitReference.setAssemblyName("nunit.core.interfaces");
    nunitReference.setVersion("2.4.8.0");
    
    assertThat(testReferences, hasItems(nunitReference));
    
  }

  @Test
  public void testProjecFiles() throws Exception {
    File file = new File(PROJECT_CORE_PATH);
    VisualStudioProject project = VisualStudioUtils.getProject(file);
    assertNotNull("Could not retrieve a project ", project);
    Collection<SourceFile> sourceFiles = project.getSourceFiles();
    log.debug("Sources : " + sourceFiles);
    assertEquals("Bad number of files extracted", 6, sourceFiles.size());
  }

  @Test
  public void testProjectFolder() throws Exception {
    File projectFile = new File(PROJECT_CORE_PATH);
    VisualStudioProject project = VisualStudioUtils.getProject(projectFile);
    File sourceFile = new File(SAMPLE_FILE_PATH);
    String relativePath = project.getRelativePath(sourceFile);
    assertThat("Invalid relative path", relativePath,
        containsString("Model" + File.separator + "SubType.cs"));
  }

  @Test
  public void testSilverlightProjecFiles() throws Exception {
    File file = new File(SILVERLIGHT_PROJECT_PATH);
    VisualStudioProject project = VisualStudioUtils.getProject(file);
    assertTrue(project.isSilverlightProject());
  }
  
  @Test
  public void testSilverlightSolution() throws Exception {
    File file = new File(SILVERLIGHT_SOLUTION_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    VisualStudioProject project = solution.getProject("BlankApplication");
    Collection<SourceFile> files = project.getSourceFiles();
    assertEquals("Bad number of files extracted", 3, files.size());
    assertTrue(project.isSilverlightProject());
    VisualStudioProject libProject = solution.getProject("BlankClassLibrary");
    Collection<SourceFile> libFiles = libProject.getSourceFiles();
    assertEquals("Bad number of files extracted", 2, libFiles.size());
  }

  @Test
  public void testReadMessySolution() throws Exception {
    File file = new File(MESSY_SOLUTION_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    VisualStudioProject project = solution.getProject("MessyTestApplication");
    Collection<SourceFile> files = project.getSourceFiles();
    assertEquals("Bad number of files extracted", 3, files.size());
    VisualStudioProject libProject = solution.getProject("ClassLibrary1");
    Collection<SourceFile> libFiles = libProject.getSourceFiles();
    assertEquals("Bad number of files extracted", 2, libFiles.size());

  }

  @Test
  public void testSolutionWithLinks() throws Exception {
    File file = new File(LINK_SOLUTION_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    List<VisualStudioProject> projects = solution.getProjects();
    for (VisualStudioProject project : projects) {
      Collection<SourceFile> files = project.getSourceFiles();
      assertEquals(
          "Bad number of files extracted for project " + project
          + " with " + StringUtils.join(files, ";"), 
          1, 
          files.size());
      
      String name = files.iterator().next().getName();

      assertThat(name, not(is("AssemblyInfo.cs")));
    }
  }
  
  @Test
  public void simpleTestPattern() {
    VisualStudioProject testProject = new VisualStudioProject();
    testProject.setAssemblyName("MyProject.Test");
    VisualStudioProject project = new VisualStudioProject();
    project.setAssemblyName("MyProject");
    VisualStudioUtils.assessTestProject(project, "*.Test");
    VisualStudioUtils.assessTestProject(testProject, "*.Test");
    assertFalse(project.isTest());
    assertTrue(testProject.isTest());
  }
  
  @Test
  public void multipleTestPatterns() {
    VisualStudioProject testProject = new VisualStudioProject();
    testProject.setAssemblyName("MyProjectTest");
    VisualStudioProject secondTestProject = new VisualStudioProject();
    secondTestProject.setAssemblyName("MyProject.IT");
    VisualStudioProject project = new VisualStudioProject();
    project.setAssemblyName("MyProject");
    
    String patterns = "*Test;*.IT";
    
    VisualStudioUtils.assessTestProject(project, patterns);
    VisualStudioUtils.assessTestProject(testProject, patterns);
    VisualStudioUtils.assessTestProject(secondTestProject, patterns);
    assertFalse(project.isTest());
    assertTrue(testProject.isTest());
    assertTrue(secondTestProject.isTest());
  }
  
  @Test
  public void testSolutionWithAssemblyNameDuplicates() throws Exception {
    File file = new File(SOLUTION_WITH_DUP_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    List<VisualStudioProject> projects = solution.getProjects();
    assertEquals(2, projects.size());
    assertFalse(projects.get(0).getAssemblyName().equals(projects.get(1).getAssemblyName()));
  }
  
  @Test
  public void testWebSolution() throws Exception {
    File file = new File(WEB_SOLUTION_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    List<VisualStudioProject> projects = solution.getProjects();
    assertEquals(2, projects.size());
    final WebVisualStudioProject webProject;
    if (projects.get(0) instanceof WebVisualStudioProject) {
      webProject = (WebVisualStudioProject) projects.get(0);
    } else {
      webProject = (WebVisualStudioProject) projects.get(1);
    }
    assertEquals(1, webProject.getReferences().size());
    Set<File> webAssemblies = webProject.getWebAssemblies();
    for (File assemblyFile : webAssemblies) {
      assertFalse("ClassLibrary.dll".equals(assemblyFile.getName()));
    }
    
  }
}
