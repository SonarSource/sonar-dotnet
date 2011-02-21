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
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.coverage.model;

import java.io.File;
import java.util.HashMap;
import java.util.Map;

/**
 * Represents the code coverage for a visual studio project.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class ProjectCoverage extends Coverable {
  private String assemblyName;
  private Map<File, FileCoverage> files = new HashMap<File, FileCoverage>();
  private int uncoveredLines = 0;

 
  /**
   * Adds a class coverage in the project
   * 
   * @param classCoverage
   */
  public void addFile(FileCoverage fileCoverage) {
    File file = fileCoverage.getFile();
    files.put(file, fileCoverage);
  }

  /**
   * Returns the assemblyName.
   * 
   * @return The assemblyName to return.
   */
  public String getAssemblyName() {
    return this.assemblyName;
  }

  /**
   * Sets the assemblyName.
   * 
   * @param assemblyName
   *          The assemblyName to set.
   */
  public void setAssemblyName(String assemblyName) {
    this.assemblyName = assemblyName;
  }

  /**
   * Summarizes the coverage
   */
  @Override
  public void summarize() {
    countLines = uncoveredLines;
    for (FileCoverage fileCoverage : files.values()) {
      countLines += fileCoverage.getCountLines();
      coveredLines += fileCoverage.getCoveredLines();
    }
  }

  @Override
  public String toString() {
    return "Project(name=" + assemblyName + ", coverage=" + getCoverage()
        + ", lines=" + countLines + ", covered=" + coveredLines + ")";
  }

}
