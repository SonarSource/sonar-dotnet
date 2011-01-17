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

import static org.junit.Assert.*;

import java.io.File;

import java.util.Collection;
import java.util.Iterator;

import org.apache.commons.lang.StringUtils;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.base.Predicate;
import com.google.common.collect.Iterators;
import com.thoughtworks.xstream.XStream;

public class GallioResultParserTest
{

	private final static Logger log = LoggerFactory.getLogger(GallioResultParserTest.class);
	
	@Test
	public void testReportParsing()
	{
	  String fileName = "gallio-report.xml";
	  GallioResultParser parser = new GallioResultStaxParser();
		//URL sampleURL = Thread.currentThread().getContextClassLoader().getResource(fileName);
		File sampleFile = new File("target/test-classes",fileName);
		Collection<UnitTestReport> reports = parser.parse(sampleFile);
		
		if(log.isDebugEnabled()){
		  log.debug(new XStream().toXML(reports));  
		}
		Predicate<UnitTestReport> reportPredicate = new Predicate<UnitTestReport>() {
			
			@Override
			public boolean apply(UnitTestReport report) {
				
				return "cs-failures".equals(report.getAssemblyName()) && StringUtils.contains(report.getSourceFile().getName(), "CSharpTest.cs") && 420==report.getTimeMS();
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
    String fileName = "gallio-report-multiple.xml";
    GallioResultParser parser = new GallioResultStaxParser();
		
		File sampleFile = new File("target/test-classes",fileName);

		Collection<UnitTestReport> reports = parser.parse(sampleFile);
    if(log.isDebugEnabled()){
      log.debug(new XStream().toXML(reports));  
    }
		final UnitTestReport randomUnit = new UnitTestReport();
		randomUnit.setAssemblyName("Example.Core.Tests");
		randomUnit.setAsserts(33);
		randomUnit.setErrors(0);
		randomUnit.setFailures(1);
		randomUnit.setSkipped(0);
		randomUnit.setTests(21);
		randomUnit.setTimeMS(81);
		
		Predicate<UnitTestReport> reportPredicate = new Predicate<UnitTestReport>() {
			@Override
			public boolean apply(UnitTestReport report) {
				
				return randomUnit.getAssemblyName().equals(report.getAssemblyName()) && 33==report.getAsserts() && 21==report.getTests();
			}
		};
		
		assertFalse("Could not parse a Gallio report", reports.isEmpty());
		Iterator<UnitTestReport> it = reports.iterator();
		
		assertTrue(it.hasNext());
		
		UnitTestReport firstReport = Iterators.find(it,reportPredicate);
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

}
