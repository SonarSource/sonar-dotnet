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
/*
 * Created on Jun 18, 2009
 *
 */
package org.sonar.plugins.csharp.gallio.results;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.util.Collection;
import java.util.Iterator;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.sonar.plugins.csharp.gallio.results.GallioResultParser;
import org.sonar.plugins.csharp.gallio.results.TestCaseDetail;
import org.sonar.plugins.csharp.gallio.results.TestStatus;
import org.sonar.plugins.csharp.gallio.results.UnitTestReport;
import org.sonar.test.TestUtils;

import com.google.common.base.Predicate;
import com.google.common.collect.Iterators;

public class GallioResultParserTest {

  private File sourcefile;
  private GallioResultParser parser;

  @Before
  public void setUp() {
    sourcefile = new File("Example\\Example.Core.Tests\\TestMoney.cs");

    parser = new GallioResultParser();
  }

  private Collection<UnitTestReport> parse(String fileName) {
    return parser.parse(TestUtils.getResource("/Results/" + fileName));
  }

  @Test
  public void testReportParsing() {
    Collection<UnitTestReport> reports = parse("gallio-report.xml");

    assertFalse("Could not parse a Gallio report", reports.isEmpty());

    Iterator<UnitTestReport> it = reports.iterator();

    assertTrue(it.hasNext());

    UnitTestReport firstReport = it.next();

    assertEquals("cs-failures", firstReport.getAssemblyName());
    assertEquals(6, firstReport.getAsserts());
    assertEquals(3, firstReport.getFailures());
    assertEquals(1, firstReport.getSkipped());
    assertTrue(StringUtils.contains(firstReport.getSourceFile().getName(), "CSharpTest.cs"));
    assertEquals(6, firstReport.getTests());
    assertEquals(420, firstReport.getTimeMS());

  }

  @Test
  public void testReportAndTestCaseDetailsParsing() {
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

    Predicate<TestCaseDetail> testCaseDetailPredicate = new Predicate<TestCaseDetail>() {

      public boolean apply(TestCaseDetail testDetail) {
        return "BagMultiply".equals(testDetail.getName()) && 3 == testDetail.getCountAsserts()
            && sourcefile.equals(testDetail.getSourceFile()) && TestStatus.SUCCESS == testDetail.getStatus()
            && 17 == testDetail.getTimeMillis();
      }
    };

    Iterator<TestCaseDetail> testCaseDetails = firstReport.getDetails().iterator();

    assertTrue(testCaseDetails.hasNext());

    assertTrue(Iterators.any(testCaseDetails, testCaseDetailPredicate));

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

  @Test
  public void testSectionBugReportParsing() {
    Collection<UnitTestReport> reports = parse("gallio-sectionbug-report.xml");
    assertTrue(reports.size() >= 1);
  }

  @Test
  public void testMbUnitReportParsing() {
    Collection<UnitTestReport> reports = parse("gallio-report-mbunit-sample.xml");
    int errors = 0;
    int failures = 0;
    int skipped = 0;
    int tests = 0;
    for (UnitTestReport unitTestReport : reports) {
      errors += unitTestReport.getErrors();
      failures += unitTestReport.getFailures();
      skipped += unitTestReport.getSkipped();
      tests += unitTestReport.getTests();
    }
    // There are 13 methods flagged as "Test" in the solution, 1 failure (1 error) and 3 tests skipped
    assertEquals(13, tests);
    assertEquals(1, failures);
    assertEquals(1, errors);
    assertEquals(3, skipped);

    // There should be 5 reports because there are 5 different test classes
    assertTrue(reports.size() >= 5);

  }

  /**
   * Test for jira ticket SONARPLUGINS-1005
   */
  @Test
  @Ignore
  public void testMsTest() {
    Collection<UnitTestReport> reports = parse("gallio-report-mstest.xml");
    assertEquals(1, reports.size());
    UnitTestReport report = reports.iterator().next();
    assertEquals(1, report.getTests());
    assertTrue(report.getTimeMS() > 25000);
  }

  public static class UnitTestReportPredicate implements Predicate<UnitTestReport> {

    private final UnitTestReport referenceReport;

    public UnitTestReportPredicate(UnitTestReport referenceReport) {
      this.referenceReport = referenceReport;
    }

    public boolean apply(UnitTestReport report) {
      return referenceReport.equals(report);
    }
  }

}
