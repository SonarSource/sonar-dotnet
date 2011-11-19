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
 *
 */
package org.sonar.plugins.csharp.gallio.results.coverage.model;

/**
 * A FileLine.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class SourceLine {

  private final int lineNumber;
  private int countVisits = 0;

  /**
   * Constructs a @link{FileLine}.
   */
  public SourceLine(int lineNumber) {
    this.lineNumber = lineNumber;
  }

  public void update(CoveragePoint point) {
    int pointVisits = point.getCountVisits();
    countVisits = countVisits + pointVisits;
  }

  /**
   * Returns the lineNumber.
   * 
   * @return The lineNumber to return.
   */
  public int getLineNumber() {
    return this.lineNumber;
  }

  /**
   * Returns the countVisits.
   * 
   * @return The countVisits to return.
   */
  public int getCountVisits() {
    return this.countVisits;
  }
}
