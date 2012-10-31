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

import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;

import com.google.common.base.Joiner;
import com.google.common.collect.Lists;
import com.google.common.collect.Maps;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.PersistenceMode;
import org.sonar.api.measures.PropertiesBuilder;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.dotnet.tools.gallio.GallioRunnerConstants;
import org.sonar.plugins.csharp.gallio.results.coverage.CoverageResultParser;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.SourceLine;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.sensor.AbstractDotNetSensor;
import org.sonar.plugins.dotnet.api.sensor.AbstractRegularDotNetSensor;
import org.sonar.plugins.dotnet.api.utils.FileFinder;

import java.io.File;
import java.util.Collection;
import java.util.List;
import java.util.Map;

/**
 * Gets the coverage test report and pushes data from it into sonar.
 */
@DependsUpon(GallioConstants.BARRIER_GALLIO_EXECUTED)
public class CoverageReportSensor extends AbstractRegularDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageReportSensor.class);

  private final PropertiesBuilder<String, Integer> lineHitsBuilder = new PropertiesBuilder<String, Integer>(
      CoreMetrics.COVERAGE_LINE_HITS_DATA);

  private final PropertiesBuilder<String, Integer> itLineHitsBuilder = new PropertiesBuilder<String, Integer>(
      CoreMetrics.IT_COVERAGE_LINE_HITS_DATA);

  private CoverageResultParser parser;

  /**
   * Constructs a {@link CoverageReportSensor}.
   *
   * @param fileSystem
   * @param configuration
   * @param microsoftWindowsEnvironment
   */
  public CoverageReportSensor(DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment,
      CoverageResultParser parser) {
    super(configuration, microsoftWindowsEnvironment, "Coverage", configuration.getString(GallioConstants.MODE));
    this.parser = parser;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public String[] getSupportedLanguages() {
    return GallioConstants.SUPPORTED_LANGUAGES;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    boolean coverageToolIsNone = GallioRunnerConstants.COVERAGE_TOOL_NONE_KEY.equals(
        configuration.getString(GallioConstants.COVERAGE_TOOL_KEY));
    return super.shouldExecuteOnProject(project) && !coverageToolIsNone;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    analyseUnitCoverage(project, context);
    analyseIntegCoverage(project, context);
  }

  public void analyseUnitCoverage(Project project, SensorContext context) {
    Collection<File> coverageReportFiles = findReportsToAnalyse(executionMode, GallioConstants.GALLIO_COVERAGE_REPORT_XML, GallioConstants.REPORTS_COVERAGE_PATH_KEY);
    if (coverageReportFiles.isEmpty()) {
      return;
    }

    // We parse the file and save the results
    parseAndSaveCoverageResults(project, context, coverageReportFiles, false);
  }

  public void analyseIntegCoverage(Project project, SensorContext context) {
    String itExecutionMode = configuration.getString(GallioConstants.IT_MODE_KEY);
    if (AbstractDotNetSensor.MODE_SKIP.equals(itExecutionMode)) {
      return;
    }
    Collection<File> coverageReportFiles = findReportsToAnalyse(itExecutionMode, GallioConstants.IT_GALLIO_COVERAGE_REPORT_XML, GallioConstants.IT_REPORTS_COVERAGE_PATH_KEY);
    if (coverageReportFiles.isEmpty()) {
      return;
    }

    // We parse the file and save the results
    parseAndSaveCoverageResults(project, context, coverageReportFiles, true);
  }

  private Collection<File> findReportsToAnalyse(String executionMode, String reportFileName, String reportPathKey) {
    Collection<File> reportFiles = Lists.newArrayList();
    File solutionDir = getVSSolution().getSolutionDir();
    String workDir = getMicrosoftWindowsEnvironment().getWorkingDirectory();
    String reportDefaultPath = workDir + "/" + reportFileName;
    if (MODE_REUSE_REPORT.equals(executionMode)) {
      String[] reportPath = configuration.getStringArray(reportPathKey, reportDefaultPath);
      reportFiles = FileFinder.findFiles(getVSSolution(), solutionDir, reportPath);
      LOG.info("Reusing Gallio it coverage reports: " + Joiner.on(" ; ").join(reportFiles));
    } else if (!getMicrosoftWindowsEnvironment().isTestExecutionDone()) {
      // This means that we are not in REUSE or SKIP mode, but for some reasons execution has not been done => skip the analysis
      LOG.info("It Coverage report analysis won't execute as Gallio was not executed.");
    } else if (configuration.getBoolean(GallioConstants.SAFE_MODE_KEY)) {
      reportFiles = FileFinder.findFiles(getVSSolution(), workDir, "*." + reportFileName);
      LOG.info("(Safe mode) Parsing Gallio it coverage reports: " + Joiner.on(" ; ").join(reportFiles));
    } else {
      File reportFile = new File(solutionDir, reportDefaultPath);
      if (reportFile.isFile()) {
        reportFiles = Lists.newArrayList(reportFile);
      } else {
        LOG.warn("No Gallio coverage report file found for: " + reportFile.getAbsolutePath());
      }
    }

    return reportFiles;
  }

  private void parseAndSaveCoverageResults(Project project, SensorContext context, Collection<File> reportFiles, boolean it) {
    Map<File, FileCoverage> fileCoverageMap = Maps.newHashMap();
    for (File report : reportFiles) {
      if (report.exists()) {
        List<FileCoverage> coverages = parser.parse(project, report);
        for (FileCoverage fileCoverage : coverages) {
          File file = fileCoverage.getFile();
          if (fileCoverageMap.containsKey(file)) {
            fileCoverageMap.get(file).merge(fileCoverage);
          } else {
            fileCoverageMap.put(file, fileCoverage);
          }
        }
      } else {
        LOG.error("Coverage report \"{}\" not found", report);
      }
    }

    // Save data for each file
    for (FileCoverage fileCoverage : fileCoverageMap.values()) {
      org.sonar.api.resources.File sonarFile = org.sonar.api.resources.File.fromIOFile(fileCoverage.getFile(), project);
      if (context.isIndexed(sonarFile, false)) {
        LOG.debug("- Saving coverage information for file {}", sonarFile.getKey());
        saveCoverageMeasures(context, fileCoverage, sonarFile, it);
        context.saveMeasure(sonarFile, getHitData(fileCoverage, it));
      }
    }
  }

  private void saveCoverageMeasures(SensorContext context, FileCoverage coverageData, Resource<?> resource, boolean it) {
    double coverage = coverageData.getCoverage();

    if (it) {
      context.saveMeasure(resource, CoreMetrics.IT_LINES_TO_COVER, (double) coverageData.getCountLines());
      context.saveMeasure(resource, CoreMetrics.IT_UNCOVERED_LINES, (double) coverageData.getCountLines() - coverageData.getCoveredLines());
      context.saveMeasure(resource, CoreMetrics.IT_COVERAGE, convertPercentage(coverage));
      // LINE_COVERAGE & COVERAGE should not be the same: need to have BRANCH_COVERAGE
      context.saveMeasure(resource, CoreMetrics.IT_LINE_COVERAGE, convertPercentage(coverage));
    } else {
      context.saveMeasure(resource, TestMetrics.ELOC, (double) coverageData.getCountLines());
      context.saveMeasure(resource, CoreMetrics.LINES_TO_COVER, (double) coverageData.getCountLines());
      context.saveMeasure(resource, CoreMetrics.UNCOVERED_LINES, (double) coverageData.getCountLines() - coverageData.getCoveredLines());
      context.saveMeasure(resource, CoreMetrics.COVERAGE, convertPercentage(coverage));
      // LINE_COVERAGE & COVERAGE should not be the same: need to have BRANCH_COVERAGE
      context.saveMeasure(resource, CoreMetrics.LINE_COVERAGE, convertPercentage(coverage));
    }
  }

  /*
   * Generates a measure that contains the visits of each line of the source file.
   */
  private Measure getHitData(FileCoverage coverable, boolean it) {
    PropertiesBuilder<String, Integer> hitsBuilder = it ? itLineHitsBuilder : this.lineHitsBuilder;

    hitsBuilder.clear();
    Map<Integer, SourceLine> lines = coverable.getLines();
    for (SourceLine line : lines.values()) {
      int lineNumber = line.getLineNumber();
      int countVisits = line.getCountVisits();
      hitsBuilder.add(Integer.toString(lineNumber), countVisits);
    }
    return hitsBuilder.build().setPersistenceMode(PersistenceMode.DATABASE);
  }

  private double convertPercentage(Number percentage) {
    return ParsingUtils.scaleValue(percentage.doubleValue() * 100.0);
  }

}
