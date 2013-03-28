/*
 * Sonar .NET Plugin :: Gallio
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
package org.sonar.plugins.csharp.gallio;

import com.google.common.base.Joiner;
import com.google.common.collect.Lists;
import com.google.common.collect.Maps;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.plugins.csharp.gallio.results.execution.GallioResultParser;
import org.sonar.plugins.csharp.gallio.results.execution.model.TestCaseDetail;
import org.sonar.plugins.csharp.gallio.results.execution.model.TestStatus;
import org.sonar.plugins.csharp.gallio.results.execution.model.UnitTestReport;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.sensor.AbstractDotNetSensor;
import org.sonar.plugins.dotnet.api.sensor.AbstractRegularDotNetSensor;
import org.sonar.plugins.dotnet.api.utils.FileFinder;

import java.io.File;
import java.util.Collection;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

/**
 * Gets the execution test report from Gallio and pushes data from it into sonar.
 */
@DependsUpon(GallioConstants.BARRIER_GALLIO_EXECUTED)
public class TestReportSensor extends AbstractRegularDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(TestReportSensor.class);

  private GallioResultParser parser;

  /**
   * Constructs a {@link TestReportSensor}.
   *
   * @param fileSystem
   * @param configuration
   * @param microsoftWindowsEnvironment
   */
  public TestReportSensor(DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment,
      GallioResultParser parser) {
    super(configuration, microsoftWindowsEnvironment, "Gallio Report Parser", configuration.getString(GallioConstants.MODE));
    this.parser = parser;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  protected boolean isTestSensor() {
    return true;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public String[] getSupportedLanguages() {
    return GallioConstants.SUPPORTED_LANGUAGES;
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {
    Collection<File> testReportFiles = findTestReportsToAnalyse();
    if (testReportFiles.isEmpty()) {
      LOG.warn("No Gallio report file found");
      context.saveMeasure(CoreMetrics.TESTS, 0.0);
      return;
    }
    collect(project, testReportFiles, context);
  }

  protected Collection<File> findTestReportsToAnalyse() {
    Collection<File> reports = Lists.newArrayList();
    reports.addAll(findReportsToAnalyse(executionMode, GallioConstants.GALLIO_REPORT_XML, GallioConstants.REPORTS_PATH_KEY));
    String itExecutionMode = configuration.getString(GallioConstants.IT_MODE_KEY);
    if (!AbstractDotNetSensor.MODE_SKIP.equals(itExecutionMode)) {
      reports.addAll(findReportsToAnalyse(itExecutionMode, GallioConstants.IT_GALLIO_REPORT_XML, GallioConstants.IT_REPORTS_PATH_KEY));
    }

    return reports;
  }

  private Collection<File> findReportsToAnalyse(String executionMode, String reportFileName, String reportPathKey) {
    Collection<File> reportFiles = Lists.newArrayList();
    File solutionDir = getVSSolution().getSolutionDir();
    String workDir = getMicrosoftWindowsEnvironment().getWorkingDirectory();
    String reportDefaultPath = workDir + "/" + reportFileName;
    if (MODE_REUSE_REPORT.equals(executionMode)) {
      String[] reportPath = configuration.getStringArray(reportPathKey, reportDefaultPath);
      reportFiles = FileFinder.findFiles(getVSSolution(), solutionDir, reportPath);
      LOG.info("Reusing Gallio coverage reports: " + Joiner.on(" ; ").join(reportFiles));
    } else {
      if (!getMicrosoftWindowsEnvironment().isTestExecutionDone()) {
        // This means that we are not in REUSE or SKIP mode, but for some reasons execution has not been done => skip the analysis
        LOG.info("Test report analysis won't execute as Gallio was not executed.");
      } else if (configuration.getBoolean(GallioConstants.SAFE_MODE_KEY)) {
        reportFiles = FileFinder.findFiles(getVSSolution(), workDir, "*." + reportFileName);
        LOG.info("(Safe mode) Parsing Gallio reports: " + Joiner.on(" ; ").join(reportFiles));
      } else {
        File reportFile = new File(solutionDir, reportDefaultPath);
        if (reportFile.isFile()) {
          reportFiles = Lists.newArrayList(reportFile);
        } else {
          LOG.warn("No Gallio report file found for: " + reportFile.getAbsolutePath());
        }
      }
    }
    return reportFiles;
  }

  private void collect(Project project, Collection<File> reportFiles, SensorContext context) {
    Map<File, UnitTestReport> fileTestMap = Maps.newHashMap();
    for (File report : reportFiles) {
      if (report.exists()) {
        Collection<UnitTestReport> tests = parser.parse(report);
        for (UnitTestReport test : tests) {
          collectTest(test, fileTestMap);
        }
      } else {
        LOG.error("Coverage report \"{}\" not found", report);
      }
    }
    LOG.debug("Found {} test data", fileTestMap.size());

    Set<File> csFilesAlreadyTreated = new HashSet<File>();

    for (UnitTestReport testReport : fileTestMap.values()) {
      saveFileMeasures(testReport, project, context, csFilesAlreadyTreated);
    }
  }

  protected void saveFileMeasures(UnitTestReport testReport, Project project, SensorContext context, Set<File> csFilesAlreadyTreated) {
    File sourceFile = testReport.getSourceFile();
    if (sourceFile != null && sourceFile.exists() && !csFilesAlreadyTreated.contains(sourceFile)) {
      LOG.debug("Collecting test data for file {}", sourceFile);
      csFilesAlreadyTreated.add(sourceFile);
      int testsCount = testReport.getTests() - testReport.getSkipped();
      org.sonar.api.resources.File testFile = fromIOFile(sourceFile, project);
      if (testFile != null) {
        saveFileMeasure(testFile, context, CoreMetrics.SKIPPED_TESTS, testReport.getSkipped());
        saveFileMeasure(testFile, context, CoreMetrics.TESTS, testsCount);
        saveFileMeasure(testFile, context, CoreMetrics.TEST_ERRORS, testReport.getErrors());
        saveFileMeasure(testFile, context, CoreMetrics.TEST_FAILURES, testReport.getFailures());
        saveFileMeasure(testFile, context, CoreMetrics.TEST_EXECUTION_TIME, testReport.getTimeMS());
        saveFileMeasure(testFile, context, TestMetrics.COUNT_ASSERTS, testReport.getAsserts());
        int passedTests = testsCount - testReport.getErrors() - testReport.getFailures();
        if (testsCount > 0) {
          double percentage = (float) passedTests * 100 / (float) testsCount;
          saveFileMeasure(testFile, context, CoreMetrics.TEST_SUCCESS_DENSITY, ParsingUtils.scaleValue(percentage));
        }
        saveTestsDetails(testFile, context, testReport);
      }
    } else {
      LOG.error("Source file not found for test report " + testReport);
    }
  }

  protected void collectTest(UnitTestReport test, Map<File, UnitTestReport> fileTestMap) {
    File file = test.getSourceFile();
    if (fileTestMap.containsKey(file)) {
      fileTestMap.get(file).merge(test);
    } else {
      fileTestMap.put(file, test);
    }
  }

  /**
   * Stores the test details in XML format.
   *
   * @param testFile
   * @param context
   * @param fileReport
   */
  private void saveTestsDetails(org.sonar.api.resources.File testFile, SensorContext context, UnitTestReport fileReport) {
    StringBuilder testCaseDetails = new StringBuilder(256);
    testCaseDetails.append("<tests-details>");
    List<TestCaseDetail> details = fileReport.getDetails();
    for (TestCaseDetail detail : details) {
      testCaseDetails.append("<testcase status=\"").append(detail.getStatus().getSonarStatus()).append("\" time=\"")
          .append(detail.getTimeMillis()).append("\" name=\"").append(detail.getName()).append("\"");
      boolean isError = (detail.getStatus() == TestStatus.ERROR);
      if (isError || (detail.getStatus() == TestStatus.FAILED)) {

        testCaseDetails.append(">").append(isError ? "<error message=\"" : "<failure message=\"").append(detail.getFormatedErrorMessage())
            .append("\">").append("<![CDATA[").append(detail.getFormatedStackTrace()).append("]]>")
            .append(isError ? "</error>" : "</failure>").append("</testcase>");
      } else {
        testCaseDetails.append("/>");
      }
    }
    testCaseDetails.append("</tests-details>");
    context.saveMeasure(testFile, new Measure(CoreMetrics.TEST_DATA, testCaseDetails.toString()));
    LOG.debug("test detail : {}", testCaseDetails);
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
  private void saveFileMeasure(org.sonar.api.resources.File testFile, SensorContext context, Metric metric, double value) {
    if (!Double.isNaN(value)) {
      context.saveMeasure(testFile, metric, value);
    }
  }

}
