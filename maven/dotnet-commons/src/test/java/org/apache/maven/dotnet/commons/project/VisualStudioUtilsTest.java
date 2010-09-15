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
import java.io.IOException;
import java.util.Collection;
import java.util.List;

import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import static org.junit.Assert.*;
import org.junit.Ignore;
import org.junit.Test;
import org.junit.matchers.JUnitMatchers;
import org.slf4j.LoggerFactory;
import org.hamcrest.CoreMatchers;

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
  
  private static final String SILVERLIGHT_PROJECT_PATH = "target/test-classes/solution/BlankSilverlightSolution/BlankApplication/BlankApplication.csproj";

  @Test
  public void testReadFiles() {
    File file = new File(PROJECT_CORE_PATH);
    List<String> files = VisualStudioUtils.getFilesPath(file);
    log.debug("Files : " + files);
    assertEquals("Bad number of files extracted", 6, files.size());
  }
  
  @Test
  @Ignore
  public void testReadSolution() throws Exception {
    File file = new File(SOLUTION_PATH);
    VisualStudioSolution solution = VisualStudioUtils.getSolution(file);
    log.debug("Solution : " + solution);
    VisualStudioProject project = solution.getProject("Example.Core");
    Collection<SourceFile> files = project.getSourceFiles();
    assertEquals("Bad number of files extracted", 6, files.size());
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
    assertThat(
        "Invalid relative path",
        relativePath,
        CoreMatchers.is(JUnitMatchers.containsString("Model" + File.separator
            + "SubType.cs")));
  }

  @Test
  public void testSilverlightProjecFiles() throws Exception {
    File file = new File(SILVERLIGHT_PROJECT_PATH);
    VisualStudioProject project = VisualStudioUtils.getProject(file);
    assertTrue(project.isSilverlightProject());
  }

}
