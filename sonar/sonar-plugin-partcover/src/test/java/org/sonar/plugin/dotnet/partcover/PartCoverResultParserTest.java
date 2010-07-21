package org.sonar.plugin.dotnet.partcover;

import static org.junit.Assert.*;

import java.net.URL;
import java.util.List;

import org.junit.Before;
import org.junit.Test;
import org.sonar.plugin.dotnet.partcover.model.FileCoverage;
import org.sonar.plugin.dotnet.partcover.model.ProjectCoverage;

public class PartCoverResultParserTest {

	private PartCoverResultParser parser;

	@Before
	public void setUp() {
		parser = new PartCoverResultParser();
	}
	
	private URL buildReportUrl(String fileName) {
		return getClass().getClassLoader().getResource(fileName);
	}
	
	@Test
	public void testParse() {
		URL url = buildReportUrl("coverage-report-2.2.xml");
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(4, files.size());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertEquals("Money.cs", firstFileCoverage.getFile().getName());
		assertEquals(45, firstFileCoverage.getCoveredLines());
	}
	
	@Test
	public void testParse23() {
		URL url = buildReportUrl("coverage-report-2.3.xml");
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(3, files.size());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertEquals("MoneyBag.cs", firstFileCoverage.getFile().getName());
		assertEquals(114, firstFileCoverage.getCoveredLines());
	}
	
	@Test
	public void testParse40() {
		URL url = buildReportUrl("coverage-report-4.0.xml");
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(2, files.size());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertEquals("Money.cs", firstFileCoverage.getFile().getName());
		assertEquals(24, firstFileCoverage.getCoveredLines());
	}

}
