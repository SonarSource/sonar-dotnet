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
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.srcmon;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.io.LineNumberReader;

import org.apache.commons.lang.StringUtils;

/**
 * 
 * Counts the number of blank lines in a source file.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class BlankLineCounter
{
  /**
   * Counts the number of blank lines.
   * 
   * @param file
   * @return the number of blank lines
   */
  public static int countBlankLines(File file)
  {
    int count = 0;
    try
    {
      FileInputStream stream = new FileInputStream(file);
      LineNumberReader reader = new LineNumberReader(new InputStreamReader(stream));
      String line;
      while ((line = reader.readLine()) != null)
      {
        if (StringUtils.isBlank(line))
        {
          count++;
        }
      }
    }
    catch (Exception e) // NOPMD
    {
      // Do nothing
    }
    return count;
  }
}
