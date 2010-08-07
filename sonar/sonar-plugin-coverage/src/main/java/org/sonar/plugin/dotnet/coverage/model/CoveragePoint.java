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
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.coverage.model;

/**
 * A coverage point.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class CoveragePoint {
  private int countVisits;
  private int startLine;
  private int endLine;

  /**
   * Constructs a @link{Point}.
   */
  public CoveragePoint() {
  }

  /**
   * Returns the countVisits.
   * 
   * @return The countVisits to return.
   */
  public int getCountVisits() {
    return this.countVisits;
  }

  /**
   * Sets the countVisits.
   * 
   * @param countVisits
   *          The countVisits to set.
   */
  public void setCountVisits(int countVisits) {
    this.countVisits = countVisits;
  }

  /**
   * Returns the startLine.
   * 
   * @return The startLine to return.
   */
  public int getStartLine() {
    return this.startLine;
  }

  /**
   * Sets the startLine.
   * 
   * @param startLine
   *          The startLine to set.
   */
  public void setStartLine(int startLine) {
    this.startLine = startLine;
  }

  /**
   * Returns the endLine.
   * 
   * @return The endLine to return.
   */
  public int getEndLine() {
    return this.endLine;
  }

  /**
   * Sets the endLine.
   * 
   * @param endLine
   *          The endLine to set.
   */
  public void setEndLine(int endLine) {
    this.endLine = endLine;
  }

  @Override
  public String toString() {
    return "Point(start-line=" + startLine + ", end-line=" + endLine
        + ", visits=" + countVisits + ")";
  }
}
