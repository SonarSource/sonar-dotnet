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
 * A Coverable.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public abstract class Coverable {

  private int countLines;
  private int coveredLines;

  /**
   * Returns the countLines.
   * 
   * @return The countLines to return.
   */
  public int getCountLines() {
    return this.countLines;
  }

  /**
   * Sets the new number of count lines
   * 
   * @param countLines
   */
  protected void setCountLines(int countLines) {
    this.countLines = countLines;
  }

  /**
   * Increases the number of count lines
   * 
   * @param numberOfLinesToAdd
   */
  protected void increaseCountLines(int numberOfLinesToAdd) {
    this.countLines += numberOfLinesToAdd;
  }

  /**
   * Returns the coveredLines.
   * 
   * @return The coveredLines to return.
   */
  public int getCoveredLines() {
    return this.coveredLines;
  }

  /**
   * Sets the new number of covered lines
   * 
   * @param coveredLines
   */
  protected void setCoveredLines(int coveredLines) {
    this.coveredLines = coveredLines;
  }

  /**
   * Increases the number of covered lines
   * 
   * @param numberOfLinesToAdd
   */
  protected void increaseCoveredLines(int coveredLines) {
    this.coveredLines += coveredLines;
  }

  /**
   * Gets the coverage ratio.
   * 
   * @return the coverage ratio
   */
  public double getCoverage() {
    if (countLines == 0) {
      return 1.;
    }
    return Math.round(((double) coveredLines / (double) countLines) * 100) * 0.01;
  }

  /**
   * Summarize the results.
   */
  public abstract void summarize();

}
