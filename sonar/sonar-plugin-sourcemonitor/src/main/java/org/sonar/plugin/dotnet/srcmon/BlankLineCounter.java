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
package org.sonar.plugin.dotnet.srcmon;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.Scanner;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 
 * Counts the number of blank lines in a source file.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class BlankLineCounter {

  private final static Logger log = LoggerFactory
      .getLogger(BlankLineCounter.class);

  /**
   * Counts the number of blank lines.
   * 
   * @param file
   * @return the number of blank lines
   */
  public static int countBlankLines(File file) {
    int count = 0;
    try {
      Scanner scanner = new Scanner(new FileReader(file));
      scanner.useDelimiter("\n");
      try {
        // first use a Scanner to get each line
        while (scanner.hasNext()) {
          String n = scanner.next();
          n = StringUtils.remove(n, "\t");
          if (StringUtils.isBlank(n)) {
            count++;
          }
        }
      } finally {
        scanner.close();
      }
    } catch (FileNotFoundException e) {
      log.error("file not found error while counting blank lines", e);
    }
    return count;
  }
}
