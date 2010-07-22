package org.sonar.plugin.dotnet.partcover;

import static org.junit.Assert.*;

import java.io.File;
import java.net.URISyntaxException;
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
		URL result = Thread.currentThread().getContextClassLoader().getResource(fileName);
		
		try {
	    assertTrue("Requested file not found", new File(result.toURI()).exists());
    } catch (URISyntaxException e) {
	    fail("Bad filename "+result);
    }
		
		return result;
	}
	
	@Test
	public void testParse() {
		URL url = buildReportUrl("coverage-report-2.2.xml");
		
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		assertEquals(1, projects.size());
		assertEquals(4, files.size());
		
		assertEquals("Example.Core", projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertEquals("Money.cs", firstFileCoverage.getFile().getName());
		assertEquals(45, firstFileCoverage.getCoveredLines());
		assertEquals(47, firstFileCoverage.getLines().size());
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
		assertEquals("MoneyBag.cs", firstFileCoverage.getFile().getName());
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
		assertEquals("Money.cs", firstFileCoverage.getFile().getName());
		assertEquals(24, firstFileCoverage.getCoveredLines());
		assertEquals(26, firstFileCoverage.getLines().size());
	}

}
