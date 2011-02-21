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

/*
 * Created on Jun 18, 2009
 *
 */
package org.sonar.plugin.dotnet.gallio;

import static org.junit.Assert.*;

import java.io.File;

import java.util.Collection;
import java.util.Iterator;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.base.Predicate;
import com.google.common.collect.Iterators;

public class GallioResultParserTest
{

	private final static Logger log = LoggerFactory.getLogger(GallioResultParserTest.class);
  private File sourcefile;
  private GallioResultParser parser;
	
  @Before
	public void setUp() {
	  sourcefile 
	    = new File("c:\\HOMEWARE\\codehaus\\dotnet\\maven\\dotnet-commons\\src\\test\\resources\\solution\\Example\\Example.Core.Tests\\TestMoney.cs");
	  parser = new GallioResultStaxParser();
	}
	
  private Collection<UnitTestReport> parse(String fileName) {
    File sampleFile = new File("target/test-classes",fileName);
    return parser.parse(sampleFile);
  }
  
  @Test
  public void testReportParsing() {
    Collection<UnitTestReport> reports = parse("gallio-report.xml");
		
		Predicate<UnitTestReport> reportPredicate = new Predicate<UnitTestReport>() {
			
			@Override
			public boolean apply(UnitTestReport report) {
				
				return "cs-failures".equals(report.getAssemblyName())
				&& StringUtils.contains(report.getSourceFile().getName(), "CSharpTest.cs") 
				&& 420==report.getTimeMS();
			}
		};
		
		assertFalse("Could not parse a Gallio report", reports.isEmpty());

		Iterator<UnitTestReport> it = reports.iterator();

		assertTrue(it.hasNext());

		UnitTestReport firstReport = Iterators.find(it,reportPredicate);

		assertEquals("cs-failures", firstReport.getAssemblyName());
		assertEquals(6, firstReport.getAsserts());
		assertEquals(3, firstReport.getFailures());
		assertEquals(1, firstReport.getSkipped());
		assertTrue(StringUtils.contains(firstReport.getSourceFile().getName(),"CSharpTest.cs"));
		assertEquals(6, firstReport.getTests());
		assertEquals(420, firstReport.getTimeMS());

	}

  @Test
  public void testReportAndTestCaseDetailsParsing()
	{
    Collection<UnitTestReport> reports = parse("gallio-report-multiple.xml");
    
		assertFalse("Could not parse a Gallio report", reports.isEmpty());
		

		final UnitTestReport expectedReport = new UnitTestReport();
    expectedReport.setAssemblyName("Example.Core.Tests");
    expectedReport.setAsserts(33);
    expectedReport.setErrors(0);
    expectedReport.setFailures(1);
    expectedReport.setSkipped(0);
    expectedReport.setTests(21);
    expectedReport.setTimeMS(81);
    expectedReport.setSourceFile(sourcefile);
		
    Iterator<UnitTestReport> it = reports.iterator();
		UnitTestReport firstReport = it.next();
    assertEquals(expectedReport, firstReport);
    
    
		final TestCaseDetail testCaseDetail = new TestCaseDetail();
		testCaseDetail.setCountAsserts(3);
		testCaseDetail.setName("BagMultiply");
		testCaseDetail.setSourceFile(new File("c:\\HOMEWARE\\codehaus\\dotnet\\maven\\dotnet-commons\\src\\test\\resources\\solution\\Example\\Example.Core.Tests\\TestMoney.cs"));
		testCaseDetail.setStatus(TestStatus.SUCCESS);
		testCaseDetail.setTimeMillis(17);
		
		Predicate<TestCaseDetail> testCaseDetailPredicate = new Predicate<TestCaseDetail>() {
			@Override
			public boolean apply(TestCaseDetail testDetail) {
				
				return testCaseDetail.equals(testDetail);
			}
		};
		
		Iterator<TestCaseDetail> testCaseDetails = firstReport.getDetails().iterator();
		
		assertTrue(testCaseDetails.hasNext());
		
		assertTrue(Iterators.any(testCaseDetails,testCaseDetailPredicate));

	}
  
  @Test
  public void testReportParsingInconclusive() {
    Collection<UnitTestReport> reports = parse("gallio-report-inconclusive.xml");
    assertEquals(1, reports.size());
    
    final UnitTestReport expectedReport = new UnitTestReport();
    expectedReport.setAssemblyName("Example.Core.Tests");
    expectedReport.setAsserts(32);
    expectedReport.setErrors(0);
    expectedReport.setFailures(1);
    expectedReport.setSkipped(1);
    expectedReport.setTests(21);
    expectedReport.setTimeMS(232);
    expectedReport.setSourceFile(sourcefile);
    
    Iterator<UnitTestReport> it = reports.iterator();
    UnitTestReport firstReport = it.next();
    assertEquals(expectedReport, firstReport);

  }
  
  public static class UnitTestReportpredicate implements Predicate<UnitTestReport> {
    
    private final UnitTestReport referenceReport;
    
    public UnitTestReportpredicate(UnitTestReport referenceReport) {
      this.referenceReport = referenceReport;
    }
    
    @Override
    public boolean apply(UnitTestReport report) {
      return referenceReport.equals(report);
    }
  }
}
