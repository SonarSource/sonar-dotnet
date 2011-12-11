package org.sonar.dotnet.tools.commons.visualstudio;

import static org.junit.Assert.*;

import java.io.File;
import java.util.List;

import org.junit.Test;
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
  
  

}
