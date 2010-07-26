package org.sonar.plugin.dotnet.coverage;

import static org.junit.Assert.*;

import java.net.URL;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Test;
import org.sonar.plugin.dotnet.coverage.CoverageResultParser;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;

public class CoverageResultParserTest {

	private CoverageResultParser parser;

	@Before
	public void setUp() {
		parser = new CoverageResultParser();
	}
	
	private URL buildReportUrl(String fileName) {
		return getClass().getClassLoader().getResource(fileName);
	}
	
	@Test
	public void testParsePartCover22() {
		ParsingParameters params = new ParsingParameters();
		params.report = "coverage-report-2.2.xml";
		params.assemblyName = "Example.Core";
		params.fileNumber = 4;
		params.fileName = "Money.cs";
		params.coveredLines = 45;
		params.lines = 47;
		
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
		
		checkParsing(params);
	}
	
	@Test
	public void testParsePartCover40() {
		ParsingParameters params = new ParsingParameters();
		params.report = "coverage-report-4.0.xml";
		params.assemblyName = "Example.Core";
		params.fileNumber = 2;
		params.fileName = "Money.cs";
		params.coveredLines = 24;
		params.lines = 26;
		
		checkParsing(params);
	}
	
	@Test
	public void testParseNCover3() {
		ParsingParameters params = new ParsingParameters();
		params.report = "Coverage.NCover3.xml";
		params.assemblyName = "Example.Core";
		params.fileNumber = 3;
		params.fileName = "Money.cs";
		params.coveredLines = 34;
		params.lines = 36;
		
		checkParsing(params);
	}
	

	
	private void checkParsing(ParsingParameters parameters) {
	  URL url = buildReportUrl(parameters.report);
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(parameters.fileNumber, files.size());
		
		
		assertEquals(parameters.assemblyName, projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		
		assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), parameters.fileName));
		
		assertEquals(parameters.coveredLines, firstFileCoverage.getCoveredLines());
		
		assertEquals(parameters.lines, firstFileCoverage.getLines().size());
  }
	
	public static class ParsingParameters {
	  public String report;
	  public String assemblyName;
	  public int fileNumber;
	  public String fileName;
	  public int coveredLines;
	  public int lines;
  }

}
