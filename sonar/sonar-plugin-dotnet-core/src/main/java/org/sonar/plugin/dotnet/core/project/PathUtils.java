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
 *
 */
package org.sonar.plugin.dotnet.core.project;


/**
 * A utility class to compute paths.
 * @author Jose CHILLAN Feb 18, 2010
 */
public class PathUtils
{
  /**
   * Computes a relative path between two directories
   * @param targetPath
   * @param basePath
   * @param pathSeparator
   * @return
   */
  public static String getRelativePath(String targetPath, String basePath, 
     String pathSeparator) {

      // find common path
      String[] target = targetPath.split(pathSeparator);
      String[] base = basePath.split(pathSeparator);

      String common = "";
      int commonIndex = 0;
      for (int i = 0; i < target.length && i < base.length; i++) {

              if (target[i].equals(base[i])) {
                      common += target[i] + pathSeparator;
                      commonIndex++;
              }
      }


      String relative = "";
      // is the target a child directory of the base directory?
      // i.e., target = /a/b/c/d, base = /a/b/
      if (commonIndex == base.length) {
              relative = "." + pathSeparator + targetPath.substring(common.length());
      }
      else {
              // determine how many directories we have to backtrack
              for (int i = 1; i <= commonIndex; i++) {
                      relative += ".." + pathSeparator;
              }
              relative += targetPath.substring(common.length());
      }

      return relative;
  }

  /**
   * Constructs the relative path between to paths.
   * @param targetPath
   * @param basePath
   * @return
   */
  public static String getRelativePath(String targetPath, String basePath) {
      return getRelativePath(targetPath, basePath, "/");
  }

}
