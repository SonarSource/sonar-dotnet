/*
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

package org.sonar.plugin.dotnet.coverage.stax;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ParserResult;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;

@Ignore
public class CoverageResultStaxParserTest {

  private CoverageResultStaxParser parser;

  @Before
  public void setUp() {
    parser = new CoverageResultStaxParser();
  }

  private File findFile(String fileName) {
    return new File("target/test-classes", fileName);
  }

  @Test
  public void testParsePartCover22() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-2.2.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 4;
    params.fileName = "Money.cs";
    params.coveredLines = 46;
    params.lines = 48;
    params.coverage = 0.96;

    checkParsing(params);
  }

  @Test
  public void testParsePartCover22Empty() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-2.2-empty.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 1;
    params.fileName = "Money.cs";
    params.coveredLines = 0;
    params.lines = 4;
    params.coverage = 0;

    checkParsing(params);
  }

  @Test
  public void testParsePartCover22Full() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-2.2-full.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 1;
    params.fileName = "Money.cs";
    params.coveredLines = 4;
    params.lines = 4;
    params.coverage = 1.0;
    
    checkParsing(params);
  }

  @Test
  public void testParsePartCover23() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-2.3.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 3;
    params.fileName = "MoneyBag.cs";
    params.coveredLines = 114;
    params.lines = 125;
    params.coverage = 0.91;
    
    checkParsing(params);
  }

  @Test
  public void testParsePartCover23NoVersionNumber() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-2.3.no.version.number.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 3;
    params.fileName = "MoneyBag.cs";
    params.coveredLines = 114;
    params.lines = 125;
    params.coverage = 0.91;
    
    checkParsing(params);
  }



  @Test
  public void testParsePartCover40NoVersionNumber() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-4.0.no.version.number.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 2;
    params.fileName = "Money.cs";
    params.coveredLines = 25;
    params.lines = 27;
    params.coverage = 0.93;
    
    checkParsing(params);
  }

  @Test
  public void testParsePartCover40() {
    ParsingParameters params = new ParsingParameters();
    params.report = "coverage-report-4.0.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 3;
    params.fileName = "Money.cs";
    params.coveredLines = 45;
    params.lines = 215;
    params.coverage = 0.21;

    checkParsing(params);
  }

  @Test
  public void testParseNCover3() {
    ParsingParameters params = new ParsingParameters();
    params.report = "Coverage.NCover3.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 3;
    params.fileName = "Money.cs";
    params.coveredLines = 35;
    params.lines = 37;
    params.coverage = 0.95;

    checkParsing(params);
  }
  
  @Test
  public void testParseNCover3WithFilter() {
    parser.setExclusionPatterns("**/Money.cs");
    ParsingParameters params = new ParsingParameters();
    params.report = "Coverage.NCover3.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 2;
    params.fileName = "MoneyBag.cs";
    params.coveredLines = 81;
    params.lines = 91;
    params.coverage = 0.89;

    checkParsing(params);
  }



  private void checkParsing(ParsingParameters parameters) {
    File file = findFile(parameters.report);
    ParserResult result = parser.parse(null, file);
    
    List<ProjectCoverage> projects = result.getProjects();
    List<FileCoverage> files = result.getSourceFiles();
    
    ProjectCoverage firstProjectCoverage = projects.get(0);
    
    assertEquals(parameters.fileNumber, files.size());

    assertEquals(parameters.assemblyName, firstProjectCoverage.getAssemblyName());
    
    FileCoverage firstFileCoverage = files.get(0);
    
    assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), parameters.fileName));

    assertEquals(parameters.coveredLines, firstFileCoverage.getCoveredLines());

    assertEquals(parameters.lines, firstFileCoverage.getCountLines());
    
    assertEquals(parameters.coverage, firstFileCoverage.getCoverage(),0.0001);
    
  }

  public static class ParsingParameters {
    public String report;
    public String assemblyName;
    public int fileNumber;
    public String fileName;
    public int coveredLines;
    public int lines;
    public double coverage;
  }

}
