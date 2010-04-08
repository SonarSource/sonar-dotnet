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
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.test.partcover;

import java.net.URL;
import java.util.List;

import junit.framework.Assert;

import org.junit.Test;
import org.sonar.plugin.dotnet.partcover.PartCoverResultParser;
import org.sonar.plugin.dotnet.partcover.model.FileCoverage;
import org.sonar.plugin.dotnet.partcover.model.ProjectCoverage;

/**
 * Tests for the {@link PartCoverResultParser} class.
 * @author Jose CHILLAN Sep 15, 2009
 */
public class TestCoverageParser
{
  @Test
  public void testFileParsing()
  {
    checkParser("coverage-report-2.2.xml", 4);
    checkParser("coverage-report-2.3.xml", 3);
  }

  public void checkParser(String resourceName, int expectedCoveredFiles)
  {
    PartCoverResultParser parser = new PartCoverResultParser();
    URL url = Thread.currentThread().getContextClassLoader().getResource(resourceName);
    parser.parse(url);
    List<FileCoverage> files = parser.getFiles();
    Assert.assertTrue("Bad number of covered files (expected " + expectedCoveredFiles + ", was " + files.size() + ")",
                      files.size() >= expectedCoveredFiles);
    System.out.println("File coverage:");
    for (FileCoverage fileCoverage : files)
    {
      System.out.println(fileCoverage);
    }
    System.out.println();
    List<ProjectCoverage> projects = parser.getProjects();
    for (ProjectCoverage projectCoverage : projects)
    {
      System.out.println(projectCoverage);
    }
    System.out.println();
  }
}
