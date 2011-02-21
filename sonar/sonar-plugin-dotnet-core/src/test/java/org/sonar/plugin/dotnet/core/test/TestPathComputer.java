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

/*
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.core.test;

import java.io.File;
import java.io.IOException;

import org.apache.commons.lang.StringUtils;
import org.junit.Test;
import org.sonar.plugin.dotnet.core.project.PathUtils;

public class TestPathComputer
{
  @Test
  public void testPathComputation()
  {
    File directory = new File("C:\\Work\\CodeQuality\\Temp\\Example\\Example.Application");
    File sourceFile= new File("C:\\Work\\CodeQuality\\Temp\\Example\\Example.Application\\Properties\\AssemblyInfo.cs");
    String relativePath;
    String absoluteSourcePath;
    String absoluteDirectoryPath;
    try
    {
      absoluteSourcePath = sourceFile.getCanonicalPath().replace('\\', '/');
      absoluteDirectoryPath = directory.getCanonicalPath().replace('\\', '/');
    }
    catch (IOException e)
    {
      return;
    }
    relativePath = PathUtils.getRelativePath(absoluteSourcePath, absoluteDirectoryPath, "/");
    String localClassName = StringUtils.removeStart(relativePath, "./").replace("/", ".");
    System.out.println("Full class name: " + localClassName);
  }
}
