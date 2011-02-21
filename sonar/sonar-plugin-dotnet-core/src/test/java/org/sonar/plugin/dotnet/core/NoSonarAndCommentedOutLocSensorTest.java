/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.core;

import static org.junit.Assert.assertEquals;

import java.io.File;

import org.junit.Ignore;
import org.junit.Test;
import org.sonar.squid.measures.Metric;
import org.sonar.squid.text.Source;

public class NoSonarAndCommentedOutLocSensorTest {

  @Test
  public void testAnalyseSourceCode() {
    File cSharpExample = new File(this.getClass().getResource("/CSharpFileExample.cs").getPath());
    Source source = NoSonarAndCommentedOutLocSensor.analyseSourceCode(cSharpExample);
    assertEquals(1, source.getNoSonarTagLines().size());
    assertEquals(9, (int) source.getNoSonarTagLines().iterator().next());

    assertEquals(5, source.getMeasure(Metric.COMMENTED_OUT_CODE_LINES));
  }
  
  @Test
  public void testAnalyseSourceCodeWithRegions() {
    File cSharpExample = new File(this.getClass().getResource("/CSharpFileExampleWithQuoteInRegion.cs").getPath());
    Source source = NoSonarAndCommentedOutLocSensor.analyseSourceCode(cSharpExample);
    assertEquals(1, source.getNoSonarTagLines().size());
    assertEquals(10, (int) source.getNoSonarTagLines().iterator().next());

    assertEquals(5, source.getMeasure(Metric.COMMENTED_OUT_CODE_LINES));
  }
  
  // TEST for SONARPLUGINS-662
  @Test
  @Ignore
  public void testAnalyseSourceCodeWithMultiLineString() {
    File cSharpExample = new File(this.getClass().getResource("/CSharpFileExampleWithMultiLineString.cs").getPath());
    Source source = NoSonarAndCommentedOutLocSensor.analyseSourceCode(cSharpExample);
    assertEquals(1, source.getNoSonarTagLines().size());
    assertEquals(10, (int) source.getNoSonarTagLines().iterator().next());

    assertEquals(5, source.getMeasure(Metric.COMMENTED_OUT_CODE_LINES));
  }
  

}
