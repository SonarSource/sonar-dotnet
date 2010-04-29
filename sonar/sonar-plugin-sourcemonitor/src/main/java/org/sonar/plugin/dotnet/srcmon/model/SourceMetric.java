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
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.srcmon.model;

/**
 * A base clase for the definition source metrics.
 * @author Jose CHILLAN Apr 6, 2010
 */
public class SourceMetric
{

  protected int countLines;
  protected int countBlankLines;
  protected int countStatements;
  protected int commentLines;
  protected int documentationLines;
  protected int countClasses;
  protected int countMethods;
  protected int countCalls;
  protected int countMethodStatements;
  protected int complexity;
  protected int countAccessors;

  /**
   * Returns the lines.
   * 
   * @return The lines to return.
   */
  public int getCountLines()
  {
    return this.countLines;
  }

  /**
   * Returns the countStatements.
   * 
   * @return The countStatements to return.
   */
  public int getCountStatements()
  {
    return this.countStatements;
  }

  /**
   * Returns the commentLines.
   * 
   * @return The commentLines to return.
   */
  public int getCommentLines()
  {
    return this.commentLines;
  }

  /**
   * Returns the documentationLines.
   * 
   * @return The documentationLines to return.
   */
  public int getDocumentationLines()
  {
    return this.documentationLines;
  }

  /**
   * Returns the countClasses.
   * 
   * @return The countClasses to return.
   */
  public int getCountClasses()
  {
    return this.countClasses;
  }

  /**
   * Returns the countMethods.
   * 
   * @return The countMethods to return.
   */
  public int getCountMethods()
  {
    return this.countMethods;
  }

  /**
   * Returns the countCalls.
   * 
   * @return The countCalls to return.
   */
  public int getCountCalls()
  {
    return this.countCalls;
  }

  /**
   * Returns the countMethodStatements.
   * 
   * @return The countMethodStatements to return.
   */
  public int getCountMethodStatements()
  {
    return this.countMethodStatements;
  }

  /**
   * Returns the countBlankLines.
   * 
   * @return The countBlankLines to return.
   */
  public int getCountBlankLines()
  {
    return this.countBlankLines;
  }

  /**
   * Returns the complexity.
   * 
   * @return The complexity to return.
   */
  public int getComplexity()
  {
    return this.complexity;
  }

  /**
   * Computes the average complexity per method
   * 
   * @return
   */
  public double getAverageComplexity()
  {
    if (countMethods == 0)
    {
      return 0;
    }
    return ((double) complexity) / countMethods;
  }

  
  /**
   * Returns the countAccessors.
   * 
   * @return The countAccessors to return.
   */
  public int getCountAccessors()
  {
    return this.countAccessors;
  }

  
  /**
   * Sets the countAccessors.
   * 
   * @param countAccessors The countAccessors to set.
   */
  public void setCountAccessors(int countAccessors)
  {
    this.countAccessors = countAccessors;
  }
}
