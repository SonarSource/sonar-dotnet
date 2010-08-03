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
 * Created on Jun 18, 2009
 *
 */
package org.sonar.plugin.dotnet.gallio;

import java.net.URL;
import java.util.Collection;

import org.junit.Assert;
import org.junit.Test;

public class TestGallioResultParser
{
  
  @Test
  public void testParsing()
  {
    checkReportFile("gallio-report.xml");
  }

  private Collection<UnitTestReport> checkReportFile(String fileName)
  {
    GallioResultParser parser = new GallioResultParser();
    URL sampleURL = Thread.currentThread().getContextClassLoader().getResource(fileName);
    Collection<UnitTestReport> reports = parser.parse(sampleURL);
    for (UnitTestReport unitTestReport : reports)
    {
      System.out.println("Report : " + unitTestReport);
    }
    Assert.assertFalse("Could not parse a Gallio report", reports.isEmpty());
    return reports;
  }

}
