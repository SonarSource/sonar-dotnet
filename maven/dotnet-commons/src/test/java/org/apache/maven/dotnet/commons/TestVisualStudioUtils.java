/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
package org.apache.maven.dotnet.commons;

import java.io.File;
import java.util.Collection;
import java.util.List;

import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.junit.Assert;
import org.junit.Test;
import org.slf4j.LoggerFactory;

/**
 * Tests for visual studio utilities.
 * @author Jose CHILLAN Sep 1, 2009
 */
public class TestVisualStudioUtils
{
  private final static org.slf4j.Logger log = LoggerFactory.getLogger(TestVisualStudioUtils.class);
  /**
   * PROJECT_CORE_PATH
   */
  private static final String PROJECT_CORE_PATH = "target/test-classes/solution/Example/Example.Core/Example.Core.csproj";
  private static final String SAMPLE_FILE_PATH = "target/test-classes/solution/Example/Example.Core/Model/SubType.cs";

  @Test
  public void testReadFiles()
  {
    File file = new File(PROJECT_CORE_PATH);
    System.out.println("File : " + file.getAbsolutePath() + ", exists: " + file.exists());
    List<String> files = VisualStudioUtils.getFilesPath(file);
    log.debug("Files : " + files);
    Assert.assertEquals("Bad number of files extracted", 6, files.size());
  }
  
  @Test
  public void testProjecFiles() throws Exception
  {
    File file = new File(PROJECT_CORE_PATH);
    VisualStudioProject project = VisualStudioUtils.getProject(file);
    Assert.assertNotNull("Could not retrieve a project ", project);
    Collection<SourceFile> sourceFiles = project.getSourceFiles();
    log.debug("Sources : " + sourceFiles);
    Assert.assertEquals("Bad number of files extracted", 6, sourceFiles.size());
  }

  @Test
  public void testProjectFolder() throws Exception
  {
    File projectFile = new File(PROJECT_CORE_PATH);
    VisualStudioProject project = VisualStudioUtils.getProject(projectFile);
    File sourceFile = new File(SAMPLE_FILE_PATH);
    String relativePath = project.getRelativePath(sourceFile);
    
    Assert.assertEquals("Invalid relative path", "Model\\SubType.cs", relativePath);
  }

}
