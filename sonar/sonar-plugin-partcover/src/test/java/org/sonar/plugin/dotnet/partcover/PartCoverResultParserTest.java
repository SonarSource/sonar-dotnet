package org.sonar.plugin.dotnet.partcover;

import static org.junit.Assert.*;

import java.net.URL;
import java.util.List;

import org.apache.commons.lang.StringUtils;
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
		String report = "coverage-report-2.2.xml";
		String assemblyName = "Example.Core";
		int fileNumber = 4;
		String fileName = "Money.cs";
		int coveredLines = 45;
		int lines = 47;
		
		
		URL url = buildReportUrl(report);
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(fileNumber, files.size());
		
		
		assertEquals(assemblyName, projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		
		assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), fileName));
		
		assertEquals(coveredLines, firstFileCoverage.getCoveredLines());
		
		assertEquals(lines, firstFileCoverage.getLines().size());
	}
	
	@Test
	public void testParse23() {
		URL url = buildReportUrl("coverage-report-2.3.xml");
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(3, files.size());
		
		assertEquals("Example.Core", projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), "MoneyBag.cs"));
		assertEquals(114, firstFileCoverage.getCoveredLines());
		assertEquals(125, firstFileCoverage.getLines().size());
	}
	
	@Test
	public void testParse40() {
		URL url = buildReportUrl("coverage-report-4.0.xml");
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(2, files.size());
		
		//assertEquals("Example.Core", projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), "Money.cs"));
		assertEquals(24, firstFileCoverage.getCoveredLines());
		assertEquals(26, firstFileCoverage.getLines().size());
	}

}
