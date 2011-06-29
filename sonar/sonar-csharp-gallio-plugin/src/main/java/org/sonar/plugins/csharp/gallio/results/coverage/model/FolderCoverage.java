/*
 * Sonar C# Plugin :: Gallio
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.plugins.csharp.gallio.results.coverage.model;

import java.io.File;
import java.util.HashMap;
import java.util.Map;

/**
 * Represents the code coverage for a folder.
 * 
 * @author Alexandre Victoor
 */
public class FolderCoverage extends Coverable {

  private String folderName;
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
   * Summarizes the coverage
   */
  @Override
  public void summarize() {
    setCountLines(uncoveredLines);
    for (FileCoverage fileCoverage : files.values()) {
      increaseCountLines(fileCoverage.getCountLines());
      increaseCoveredLines(fileCoverage.getCoveredLines());
    }
  }

  @Override
  public String toString() {
    return "Folder(name=" + folderName + ", coverage=" + getCoverage() + ", lines=" + getCountLines() + ", covered=" + getCoveredLines()
        + ")";
  }

  public String getFolderName() {
    return folderName;
  }

  public void setFolderName(String folderName) {
    this.folderName = folderName;
  }

}
