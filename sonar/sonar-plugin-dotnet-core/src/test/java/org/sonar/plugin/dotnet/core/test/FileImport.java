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
 * Created on Sep 29, 2009
 */
package org.sonar.plugin.dotnet.core.test;

import java.io.File;

import org.apache.commons.io.FileUtils;

/**
 * A FileImport.
 * 
 * @author Jose CHILLAN Sep 29, 2009
 */
public class FileImport
{
  public static void main(String[] args) throws Exception
  {
    File file = new File("C:/Work/CodeQuality/projects/dotnet-commons/src/test/resources/solution/Example/Example.Application/Program.cs");
    String content = FileUtils.readFileToString(file, "UTF-8").substring(0, 50);
    for (int idxChar = 0; idxChar < content.length(); idxChar++)
    {
      char chr = content.charAt(idxChar);
      System.out.println("'" + chr + "' : " + Integer.toHexString(chr));
    }
    if (content.startsWith("\uFEFF") || content.startsWith("\uFFFE"))
    {
      content = content.substring(1);
    }
    System.out.println();
    System.out.println(content);
  }
}
