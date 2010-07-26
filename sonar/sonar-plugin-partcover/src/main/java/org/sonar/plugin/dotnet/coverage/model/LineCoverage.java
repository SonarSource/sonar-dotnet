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
 *
 */
package org.sonar.plugin.dotnet.coverage.model;

/**
 * Coverage results for a line
 * @author Jose CHILLAN May 14, 2009
 */
public class LineCoverage
{
  private int lineNumber;
  private int visit;
  
  /**
   * Constructs a @link{LineCoverage}.
   */
  public LineCoverage()
  {
  }

  
  /**
   * Returns the lineNumber.
   * 
   * @return The lineNumber to return.
   */
  public int getLineNumber()
  {
    return this.lineNumber;
  }

  
  /**
   * Sets the lineNumber.
   * 
   * @param lineNumber The lineNumber to set.
   */
  public void setLineNumber(int lineNumber)
  {
    this.lineNumber = lineNumber;
  }

  
  /**
   * Returns the visit.
   * 
   * @return The visit to return.
   */
  public int getVisit()
  {
    return this.visit;
  }

  
  /**
   * Sets the visit.
   * 
   * @param visit The visit to set.
   */
  public void setVisit(int visit)
  {
    this.visit = visit;
  }
  
  
}
