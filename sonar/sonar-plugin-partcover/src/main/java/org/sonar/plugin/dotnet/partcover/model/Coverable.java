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
package org.sonar.plugin.dotnet.partcover.model;

/**
 * A Coverable.
 * @author Jose CHILLAN May 14, 2009
 */
public abstract class Coverable
{
  protected int countLines;
  protected int coveredLines;

  /**
   * Constructs a @link{Coverable}.
   */
  public Coverable()
  {
  }

  /**
   * Returns the countLines.
   * 
   * @return The countLines to return.
   */
  public int getCountLines()
  {
    return this.countLines;
  }

  /**
   * Returns the coveredLines.
   * 
   * @return The coveredLines to return.
   */
  public int getCoveredLines()
  {
    return this.coveredLines;
  }

  /**
   * Gets the coverage ratio.
   * @return the coverage ratio
   */
  public double getCoverage()
  {
    if (countLines== 0)
    {
      return 1.;
    }
    return Math.round(((double) coveredLines / (double) countLines) * 100)*0.01;
  }

  /**
   * Summarize the results.
   */
  public abstract void summarize();

}
