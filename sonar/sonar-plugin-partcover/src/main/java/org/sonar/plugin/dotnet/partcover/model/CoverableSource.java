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

import java.util.HashMap;
import java.util.Map;

/**
 * A Coverable object (mainly a class or file)
 * @author Jose CHILLAN May 14, 2009
 */
public class CoverableSource extends Coverable
{

  protected Map<Integer, SourceLine> lines;
  /**
   * Constructs a @link{Coverable}.
   */
  public CoverableSource()
  {
    lines = new HashMap<Integer, SourceLine>();
  }

  /**
   * Adds a line coverage.
   * @param lineCoverage
   */
  public void addPoint(CoveragePoint point)
  {
    int startLine = point.getStartLine();
    int endLine = point.getStartLine();
    for(int idx = startLine; idx <= endLine; idx++)
    {
      // We add a point for each line
      SourceLine line = lines.get(startLine);
      if (line == null)
      {
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
  public void summarize()
  {
    countLines = lines.size();
    coveredLines = 0;
    for (SourceLine line : lines.values())
    {
      if (line.getCountVisits() > 0)
      {
        coveredLines++;
      }
    }
  }

  
  /**
   * Returns the lines.
   * 
   * @return The lines to return.
   */
  public Map<Integer, SourceLine> getLines()
  {
    return this.lines;
  }
}
