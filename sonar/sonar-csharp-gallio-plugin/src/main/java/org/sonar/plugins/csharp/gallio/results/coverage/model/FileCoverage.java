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
 * A FileCoverage.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class FileCoverage extends Coverable {
  private final File file;
  private String assemblyName;
  private Map<Integer, SourceLine> lines = new HashMap<Integer, SourceLine>();
  private int uncoveredLines = 0;


  /**
   * Constructs a @link{FileCoverage}.
   */
  public FileCoverage(File file) {
    this.file = file;
    this.lines = new HashMap<Integer, SourceLine>();
  }
  
  /**
   * Increase the counter of uncovered lines. Usually happens
   * wih partcover4 when a whole method has not been tested
   * @param lines
   */
  public void addUncoveredLines(int lines) {
    uncoveredLines += lines;
  }

  /**
   * Returns the file.
   * 
   * @return The file to return.
   */
  public File getFile() {
    return this.file;
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
   * Adds a line coverage.
   * 
   * @param lineCoverage
   */
  public void addPoint(CoveragePoint point) {
    int startLine = point.getStartLine();
    int endLine = point.getEndLine();
    for (int idx = startLine; idx <= endLine; idx++) {
      // We add a point for each line
      SourceLine line = lines.get(idx);
      if (line == null) {
        line = new SourceLine(idx);
        lines.put(idx, line);
      }
      line.update(point);
    }
  }

  /**
   * Summarize the results
   */
  @Override
  public void summarize() {
    countLines = lines.size() + uncoveredLines;
    coveredLines = 0;
    for (SourceLine line : lines.values()) {
      if (line.getCountVisits() > 0) {
        coveredLines++;
      }
    }
  }

  /**
   * Returns the lines.
   * 
   * @return The lines to return.
   */
  public Map<Integer, SourceLine> getLines() {
    return this.lines;
  }
 

  @Override
  public String toString() {
    return "File(name=" + file.getName() + ", assembly=" + assemblyName
        + ", coverage=" + getCoverage() + ", lines=" + countLines
        + ", covered=" + coveredLines + ")";
  }
}
