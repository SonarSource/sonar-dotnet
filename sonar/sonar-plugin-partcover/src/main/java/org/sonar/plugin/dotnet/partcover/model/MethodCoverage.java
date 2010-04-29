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
package org.sonar.plugin.dotnet.partcover.model;

import java.util.ArrayList;
import java.util.List;

/**
 * Coverage results for a method
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class MethodCoverage
{
  private String             name;
  private List<LineCoverage> lines;

  /**
   * Constructs a @link{MethodCoverage}.
   */
  public MethodCoverage()
  {
    super();
    lines = new ArrayList<LineCoverage>();
  }

  /**
   * Returns the name.
   * 
   * @return The name to return.
   */
  public String getName()
  {
    return this.name;
  }

  /**
   * Sets the name.
   * 
   * @param name The name to set.
   */
  public void setName(String name)
  {
    this.name = name;
  }

  /**
   * Adds a line coverage.
   * 
   * @param lineCoverage
   */
  public void addLine(LineCoverage lineCoverage)
  {
    lines.add(lineCoverage);
  }

  /**
   * Returns the lines.
   * 
   * @return The lines to return.
   */
  public List<LineCoverage> getLines()
  {
    return this.lines;
  }

  /**
   * Sets the lines.
   * 
   * @param lines The lines to set.
   */
  public void setLines(List<LineCoverage> lines)
  {
    this.lines = lines;
  }

}
