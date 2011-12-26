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
package org.sonar.plugins.csharp.gallio.results.coverage.model;

import static org.junit.Assert.*;

import org.junit.Test;

public class FileCoverageTest {

  @Test
  public void shouldMergeCoverage() {
    FileCoverage firstCoverage = new FileCoverage(null);
    CoveragePoint point = new CoveragePoint();
    point.setCountVisits(2);
    point.setStartLine(3);
    point.setEndLine(10);
    firstCoverage.addPoint(point);
    
    FileCoverage secondCoverage = new FileCoverage(null);
    point = new CoveragePoint();
    point.setCountVisits(3);
    point.setStartLine(7);
    point.setEndLine(15);
    secondCoverage.addPoint(point);
    point = new CoveragePoint();
    point.setCountVisits(0);
    point.setStartLine(9);
    point.setEndLine(20);
    secondCoverage.addPoint(point);
    
    firstCoverage.merge(secondCoverage);
    
    assertEquals(18, firstCoverage.getCountLines());
    assertEquals(13, firstCoverage.getCoveredLines());
    assertEquals(13d/18d, firstCoverage.getCoverage(), 0.01d);
    
    SourceLine intersectionLine = firstCoverage.getLines().get(8);
    assertEquals(5, intersectionLine.getCountVisits());
    
    SourceLine disjunctionLine = firstCoverage.getLines().get(4);
    assertEquals(2, disjunctionLine.getCountVisits());
  }
}
