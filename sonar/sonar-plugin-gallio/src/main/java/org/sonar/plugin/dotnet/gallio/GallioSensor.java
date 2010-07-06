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
 * Created on Jun 4, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import java.io.File;
import java.util.List;

import javax.xml.transform.TransformerException;

import org.apache.commons.lang.StringEscapeUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.plugin.dotnet.core.AbstractDotnetSensor;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;

/**
 * Collects the result of Gallio analysis into sonar.
 * 
 * @author Jose CHILLAN Jun 4, 2009
 */
public class GallioSensor extends AbstractDotnetSensor {
	private final static Logger log = LoggerFactory.getLogger(GallioSensor.class);

	public static final String GALLIO_REPORT_XML = "gallio-report.xml";

	/**
	 * Constructs a @link{GallioCollector}.
	 */
	public GallioSensor() {
	}

	/**
	 * collect
	 * 
	 * @param pom
	 * @param context
	 */
	/**
	 * @param project
	 * @param context
	 */
	@Override
	public void analyse(Project project, SensorContext context) {
		// Retrieve the report
		File report = findReport(project, GALLIO_REPORT_XML);
		if (report == null) {
			context.saveMeasure(CoreMetrics.TEST_DATA, 0.0);
		} else {
			collect(project, report, context);
		}
	}

	/**
	 * @param project
	 * @return
	 */
	@Override
	public MavenPluginHandler getMavenPluginHandler(Project project) {
		return new GallioMavenPluginHandler();
	}

	/**
	 * @param project
	 * @return
	 */
	@Override
	public boolean shouldExecuteOnProject(Project project) {
		String packaging = project.getPackaging();
		// We only accept the "sln" packaging
		return "sln".equals(packaging);
	}

	private void collect(Project project, File report, SensorContext context) {
		GallioResultParser parser = new GallioResultParser();
		List<UnitTestReport> reports = parser.parse(report);
		for (UnitTestReport testReport : reports) {
			File sourceFile = testReport.getSourceFile();
			if (sourceFile != null && sourceFile.exists()) {
				int testsCount = testReport.getTests() - testReport.getSkipped();
				CSharpFile testFile = 
					CSharpFile.from(project, testReport.getSourceFile(), true);
				saveFileMeasure(testFile, context, testReport, CoreMetrics.SKIPPED_TESTS, testReport.getSkipped());
				saveFileMeasure(testFile, context, testReport, CoreMetrics.TESTS, testsCount);
				saveFileMeasure(testFile, context, testReport, CoreMetrics.TEST_ERRORS, testReport.getErrors());
				saveFileMeasure(testFile, context, testReport, CoreMetrics.TEST_FAILURES, testReport.getFailures());
				saveFileMeasure(testFile, context, testReport, CoreMetrics.TEST_EXECUTION_TIME, testReport.getTimeMS());
				saveFileMeasure(testFile, context, testReport, GallioMetrics.COUNT_ASSERTS, testReport.getAsserts());
				int passedTests = testsCount - testReport.getErrors() - testReport.getFailures();
				if (testsCount > 0) {
					double percentage = passedTests * 100 / testsCount;
					saveFileMeasure(testFile, context, testReport, CoreMetrics.TEST_SUCCESS_DENSITY, ParsingUtils.scaleValue(percentage));
				}
				try {
					saveTestsDetails(testFile, context, testReport);
				} catch (TransformerException e) {
					// We discard the exception
				}
			} else {
				log.error("Source file not found for test report " + testReport);
			}
		}
	}

	/**
	 * Stores the test details in XML format.
	 * 
	 * @param testFile
	 * @param context
	 * @param fileReport
	 * @throws TransformerException
	 */
	private void saveTestsDetails(CSharpFile testFile, SensorContext context,
	    UnitTestReport fileReport) throws TransformerException {
		StringBuilder testCaseDetails = new StringBuilder(256);
		testCaseDetails.append("<tests-details>");
		List<TestCaseDetail> details = fileReport.getDetails();
		for (TestCaseDetail detail : details) {
			testCaseDetails.append("<testcase status=\"").append(detail.getStatus())
			    .append("\" time=\"").append(detail.getTimeMillis()).append(
			        "\" name=\"").append(detail.getName()).append("\"").append(
			        "\" asserts=\"").append(detail.getCountAsserts()).append("\"");
			boolean isError = (detail.getStatus() == TestStatus.ERROR);
			if (isError || (detail.getStatus() == TestStatus.FAILED)) {
				testCaseDetails.append(">").append(
				    isError ? "<error message=\"" : "<failure message=\"").append(
				    StringEscapeUtils.escapeXml(detail.getErrorMessage()))
				    .append("\">").append("<![CDATA[").append(detail.getStackTrace())
				    .append("]]>").append(isError ? "</error>" : "</failure>").append(
				        "</testcase>");
			} else {
				testCaseDetails.append("/>");
			}
		}
		testCaseDetails.append("</tests-details>");
		context.saveMeasure(testFile, new Measure(CoreMetrics.TEST_DATA,
		    testCaseDetails.toString()));
	}

	/**
	 * Saves the measure the a test file.
	 * 
	 * @param project
	 * @param context
	 * @param fileReport
	 * @param metric
	 * @param value
	 */
	private void saveFileMeasure(CSharpFile testFile, SensorContext context,
	    UnitTestReport fileReport, Metric metric, double value) {
		if (!Double.isNaN(value)) {
			context.saveMeasure(testFile, metric, value);
		}
	}

}
