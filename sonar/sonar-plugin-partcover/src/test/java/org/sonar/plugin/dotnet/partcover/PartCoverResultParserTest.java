package org.sonar.plugin.dotnet.partcover;

import static org.junit.Assert.*;

import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.partcover.model.FileCoverage;
import org.sonar.plugin.dotnet.partcover.model.ProjectCoverage;

public class PartCoverResultParserTest {
	
	private final static Logger log = LoggerFactory.getLogger(PartCoverResultParserTest.class);

	private PartCoverResultParser parser;

	@Before
	public void setUp() {
		parser = new PartCoverResultParser();
	}
	
	private URL buildReportUrl(String fileName) {
		//log.de
		URL result = Thread.currentThread().getContextClassLoader().getResource(fileName);
		
		try {
			System.out.println("url generated : "+result);
			System.out.println("file exist ? " + new File(result.toURI()).exists());
	    assertTrue("Requested file not found", new File(result.toURI()).exists());
    } catch (URISyntaxException e) {
	    fail("Bad filename "+result);
    }
		
		return result;
	}
	
	@Test
	public void testParse() {
		try {
			System.out.println("res parse partcover 2.2");
		URL url = buildReportUrl("coverage-report-2.2.xml");
		
		parser.parse(url);
		System.out.println("after parsing");
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();
		
		System.out.println("projects.size() " + projects.size());
		System.out.println("files.size() " + files.size());
		System.out.println("projects.get(0).getAssemblyName() " + projects.get(0).getAssemblyName() );
		
		assertEquals(1, projects.size());
		assertEquals(4, files.size());
		
		assertEquals("Example.Core", projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		System.out.println("firstFileCoverage "+firstFileCoverage);
		System.out.println("firstFileCoverage.getFile().getName() "+firstFileCoverage.getFile().getName());
		assertTrue(StringUtils.contains(firstFileCoverage.getFile().getName(), "Money.cs"));
		System.out.println("firstFileCoverage.getCoveredLines() "+firstFileCoverage.getCoveredLines());
		assertEquals(45, firstFileCoverage.getCoveredLines());
		System.out.println("firstFileCoverage.getLines().size() "+firstFileCoverage.getLines().size());
		assertEquals(47, firstFileCoverage.getLines().size());
		} catch (RuntimeException ex) {
			ex.printStackTrace();
			log.error("test error", ex);
			throw ex;
		}
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
		
		assertEquals("Example.Core", projects.get(0).getAssemblyName());
		
		FileCoverage firstFileCoverage = files.get(0);
		assertEquals("Money.cs", firstFileCoverage.getFile().getName());
		assertEquals(24, firstFileCoverage.getCoveredLines());
		assertEquals(26, firstFileCoverage.getLines().size());
	}

}
