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
package org.sonar.plugins.csharp.gallio.results.coverage;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.any;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.resources.Resource;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ParserResult;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ProjectCoverage;
import org.sonar.test.TestUtils;

import com.google.inject.internal.util.Lists;

public class CoverageResultParserTest {

  private CoverageResultParser parser;
  private SensorContext context;
  private Project project;

  @Before
  public void setUp() {
    context = mock(SensorContext.class);
    when(context.isIndexed(any(Resource.class), eq(false))).thenReturn(true);
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSourceDirs()).thenReturn(Lists.newArrayList(new File("C:\\Work\\CodeQuality\\Temp\\Example")));
    project = mock(Project.class);
    when(project.getFileSystem()).thenReturn(fileSystem);
    parser = new CoverageResultParser(context);
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
  @Ignore
  public void testParseOpenCover() {
    ParsingParameters params = new ParsingParameters();
    params.report = "Coverage.OpenCover.xml";
    params.assemblyName = "Example.Core";
    params.fileNumber = 3;
    params.fileName = "Money.cs";
    params.coveredLines = 45;
    params.lines = 47;
    params.coverage = 0.96;

    checkParsing(params);
  }
  
  @Test
  public void parseEmptyPartCoverReport() {
	File file = TestUtils.getResource("/Results/coverage/empty-partcover-report.xml");
	ParserResult result = parser.parse(project, file);
	assertTrue(result.getSourceFiles().isEmpty());
  }

  private void checkParsing(ParsingParameters parameters) {
    File file = TestUtils.getResource("/Results/coverage/" + parameters.report);
    ParserResult result = parser.parse(project, file);

    List<ProjectCoverage> projects = result.getProjects();
    List<FileCoverage> files = result.getSourceFiles();

    int numberOfLinesInProjects = 0;
    for (ProjectCoverage projectCoverage : projects) {
      numberOfLinesInProjects += projectCoverage.getCountLines();
    }

    int numberOfLinesInFiles = 0;
    for (FileCoverage fileCoverage : files) {
      numberOfLinesInFiles += fileCoverage.getCountLines();
    }

    assertEquals("line number in projects and files do not match", numberOfLinesInFiles, numberOfLinesInProjects);

    ProjectCoverage firstProjectCoverage = projects.get(0);

    assertEquals(parameters.fileNumber, files.size());

    assertEquals(parameters.assemblyName, firstProjectCoverage.getAssemblyName());

    FileCoverage firstFileCoverage = files.get(0);

    assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), parameters.fileName));

    assertEquals(parameters.coveredLines, firstFileCoverage.getCoveredLines());

    assertEquals(parameters.lines, firstFileCoverage.getCountLines());

    assertEquals(parameters.coverage, firstFileCoverage.getCoverage(), 0.0001);

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
