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
/*
 * Created on Sep 1, 2009
 *
 */
package org.sonar.plugins.csharp.api.visualstudio;

import static org.hamcrest.CoreMatchers.is;
import static org.hamcrest.CoreMatchers.not;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.junit.matchers.JUnitMatchers.containsString;
import static org.junit.matchers.JUnitMatchers.hasItems;

import java.io.File;
import java.util.Collection;
import java.util.List;
import java.util.Set;

import org.apache.commons.lang.StringUtils;
import org.junit.Test;
import org.slf4j.LoggerFactory;

/**
 * Tests for visual studio utilities.
 * 
 * @author Fabrice BELLINGARD
 * @author Jose CHILLAN Sep 1, 2009
 */
public class ModelFactoryTest {

  private static final org.slf4j.Logger log = LoggerFactory.getLogger(ModelFactoryTest.class);
  /**
   * PROJECT_CORE_PATH
   */
  private static final String PROJECT_CORE_PATH = "target/test-classes/solution/Example/Example.Core/Example.Core.csproj";
  private static final String SAMPLE_FILE_PATH = "target/test-classes/solution/Example/Example.Core/Model/SubType.cs";
  private static final String SOLUTION_PATH = "target/test-classes/solution/Example/Example.sln";
  private static final String MESSY_SOLUTION_PATH = "target/test-classes/solution/MessyTestSolution/MessyTestSolution.sln";
  private static final String LINK_SOLUTION_PATH = "target/test-classes/solution/LinkTestSolution/LinkTestSolution.sln";
  private static final String SOLUTION_WITH_DUP_PATH = "target/test-classes/solution/DuplicatesExample/Example.sln";
  private static final String SOLUTION_WITH_CUSTOM_BUILD_PATH = "target/test-classes/solution/CustomBuild/CustomBuild.sln";

  private static final String SILVERLIGHT_SOLUTION_PATH = "target/test-classes/solution/BlankSilverlightSolution/BlankSilverlightSolution.sln";
  private static final String SILVERLIGHT_PROJECT_PATH = "target/test-classes/solution/BlankSilverlightSolution/BlankApplication/BlankApplication.csproj";

  private static final String WEB_SOLUTION_PATH = "target/test-classes/solution/web-solution/web-solution.sln";

  @Test
  public void testReadFiles() {
    File file = new File(PROJECT_CORE_PATH);
    List<String> files = ModelFactory.getFilesPath(file);
    log.debug("Files : " + files);
    assertEquals("Bad number of files extracted", 6, files.size());
  }

  @Test
  public void testSolutionWithCustomBuild() throws Exception {
    File file = new File(SOLUTION_WITH_CUSTOM_BUILD_PATH);
    VisualStudioSolution solution = ModelFactory.getSolution(file);
    List<String> buildConfigurations = solution.getBuildConfigurations();
    assertEquals(3, buildConfigurations.size());
    assertTrue(buildConfigurations.contains("Debug"));
    assertTrue(buildConfigurations.contains("Release"));
    assertTrue(buildConfigurations.contains("CustomCompil"));

    assertEquals(1, solution.getProjects().size());
    VisualStudioProject project = solution.getProjects().get(0);

    // Debug configuration should be the preferred one
    assertTrue(project.getArtifact("CustomCompil;Debug").getAbsolutePath().contains("Debug"));

    assertTrue(project.getArtifact("CustomCompil").getAbsolutePath().contains("CustomCompil"));
    assertTrue(project.getArtifact("CustomCompil").getAbsolutePath().endsWith(project.getName() + ".dll"));

  }

  @Test
  public void testReadSolution() throws Exception {
    File file = new File(SOLUTION_PATH);
    VisualStudioSolution solution = ModelFactory.getSolution(file);
    log.debug("Solution : " + solution);
    VisualStudioProject project = solution.getProject("Example.Core");
    Collection<SourceFile> files = project.getSourceFiles();
    assertEquals("Bad number of files extracted", 6, files.size());
    List<BinaryReference> references = project.getBinaryReferences();
    assertTrue(references.size() > 0);

    BinaryReference systemReference = new BinaryReference();
    systemReference.setAssemblyName("System.Xml.Linq");
    systemReference.setVersion("v3.5");
    systemReference.setScope("compile");

    assertThat(references, hasItems(systemReference));

    VisualStudioProject testProject = solution.getProject("Example.Core.Tests");
    assertThat(testProject.isTest(), is(true));
    List<BinaryReference> testReferences = testProject.getBinaryReferences();
    BinaryReference nunitReference = new BinaryReference();
    nunitReference.setAssemblyName("nunit.core.interfaces");
    nunitReference.setVersion("2.4.8.0");
    nunitReference.setScope("test");

    assertThat(testReferences, hasItems(nunitReference));

    // same test with the core nunit reference, defined a litle bit differently in
    // the csproj file
    BinaryReference nunitCoreReference = new BinaryReference();
    nunitCoreReference.setAssemblyName("nunit.core");
    nunitCoreReference.setVersion("2.4.8.0");
    nunitCoreReference.setScope("test");

    assertThat(testReferences, hasItems(nunitCoreReference));

  }

  @Test
  public void testProjecFiles() throws Exception {
    File file = new File(PROJECT_CORE_PATH);
    VisualStudioProject project = ModelFactory.getProject(file);
    assertNotNull("Could not retrieve a project ", project);
    Collection<SourceFile> sourceFiles = project.getSourceFiles();
    log.debug("Sources : " + sourceFiles);
    assertEquals("Bad number of files extracted", 6, sourceFiles.size());
  }

  @Test
  public void testProjectFolder() throws Exception {
    File projectFile = new File(PROJECT_CORE_PATH);
    VisualStudioProject project = ModelFactory.getProject(projectFile);
    File sourceFile = new File(SAMPLE_FILE_PATH);
    String relativePath = project.getRelativePath(sourceFile);
    assertThat("Invalid relative path", relativePath, containsString("Model" + File.separator + "SubType.cs"));
  }

  @Test
  public void testSilverlightProjecFiles() throws Exception {
    File file = new File(SILVERLIGHT_PROJECT_PATH);
    VisualStudioProject project = ModelFactory.getProject(file);
    assertTrue(project.isSilverlightProject());
  }

  @Test
  public void testSilverlightSolution() throws Exception {
    File file = new File(SILVERLIGHT_SOLUTION_PATH);
    VisualStudioSolution solution = ModelFactory.getSolution(file);
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
    VisualStudioSolution solution = ModelFactory.getSolution(file);
    log.debug("Solution : " + solution);
    VisualStudioProject project = solution.getProject("MessyTestApplication");
    Collection<SourceFile> files = project.getSourceFiles();
    assertEquals("Bad number of files extracted", 3, files.size());
    VisualStudioProject libProject = solution.getProject("ClassLibrary1");
    assertFalse(project.isSilverlightProject());
    Collection<SourceFile> libFiles = libProject.getSourceFiles();
    assertEquals("Bad number of files extracted", 2, libFiles.size());

  }

  @Test
  public void testSolutionWithLinks() throws Exception {
    File file = new File(LINK_SOLUTION_PATH);
    VisualStudioSolution solution = ModelFactory.getSolution(file);
    log.debug("Solution : " + solution);
    List<VisualStudioProject> projects = solution.getProjects();
    for (VisualStudioProject project : projects) {
      Collection<SourceFile> files = project.getSourceFiles();
      assertEquals("Bad number of files extracted for project " + project + " with " + StringUtils.join(files, ";"), 1, files.size());

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
    ModelFactory.assessTestProject(project, "*.Test");
    ModelFactory.assessTestProject(testProject, "*.Test");
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

    ModelFactory.assessTestProject(project, patterns);
    ModelFactory.assessTestProject(testProject, patterns);
    ModelFactory.assessTestProject(secondTestProject, patterns);
    assertFalse(project.isTest());
    assertTrue(testProject.isTest());
    assertTrue(secondTestProject.isTest());
  }

  @Test
  public void testSolutionWithAssemblyNameDuplicates() throws Exception {
    File file = new File(SOLUTION_WITH_DUP_PATH);
    VisualStudioSolution solution = ModelFactory.getSolution(file);
    log.debug("Solution : " + solution);
    List<VisualStudioProject> projects = solution.getProjects();
    assertEquals(2, projects.size());
    assertFalse(projects.get(0).getAssemblyName().equals(projects.get(1).getAssemblyName()));
  }

  @Test
  public void testWebSolution() throws Exception {
    if ('/' == File.separatorChar) {
      // test does not work on linux boxes
      return;
    }

    File file = new File(WEB_SOLUTION_PATH);
    VisualStudioSolution solution = ModelFactory.getSolution(file);
    log.debug("Solution : " + solution);
    List<VisualStudioProject> projects = solution.getProjects();
    assertEquals(2, projects.size());
    final VisualStudioWebProject webProject;
    if (projects.get(0) instanceof VisualStudioWebProject) {
      webProject = (VisualStudioWebProject) projects.get(0);
    } else {
      webProject = (VisualStudioWebProject) projects.get(1);
    }
    assertEquals(1, webProject.getReferences().size());
    Set<File> webAssemblies = webProject.getGeneratedAssemblies(null);
    for (File assemblyFile : webAssemblies) {
      assertFalse("ClassLibrary.dll".equals(assemblyFile.getName()));
    }
  }
}
