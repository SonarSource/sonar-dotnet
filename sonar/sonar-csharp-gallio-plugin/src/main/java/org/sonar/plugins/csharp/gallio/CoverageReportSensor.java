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
package org.sonar.plugins.csharp.gallio;

import java.io.File;
import java.util.Collection;
import java.util.List;
import java.util.Map;

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
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractRegularCSharpSensor;
import org.sonar.plugins.csharp.gallio.results.coverage.CoverageResultParser;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.SourceLine;

import com.google.common.base.Joiner;
import com.google.common.collect.Lists;
import com.google.common.collect.Maps;

/**
 * Gets the coverage test report and pushes data from it into sonar.
 */
@DependsUpon(GallioConstants.BARRIER_GALLIO_EXECUTED)
public class CoverageReportSensor extends AbstractRegularCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageReportSensor.class);

  private final PropertiesBuilder<String, Integer> lineHitsBuilder = new PropertiesBuilder<String, Integer>(
      CoreMetrics.COVERAGE_LINE_HITS_DATA);

  private CoverageResultParser parser;
  private CSharpConfiguration configuration;

  /**
   * Constructs a {@link CoverageReportSensor}.
   * 
   * @param fileSystem
   * @param configuration
   * @param microsoftWindowsEnvironment
   */
  public CoverageReportSensor(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment,
      CoverageResultParser parser) {
    super(microsoftWindowsEnvironment, "Coverage", configuration.getString(GallioConstants.MODE, ""));
    this.configuration = configuration;
    this.parser = parser;
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    Collection<File> coverageReportFiles = findCoverageReportsToAnalyse();
    if (coverageReportFiles.isEmpty()) {
      return;
    }

    // We parse the file and save the results
    parseAndSaveCoverageResults(project, context, coverageReportFiles);
  }

  private Collection<File> findCoverageReportsToAnalyse() {
    Collection<File> reportFiles = Lists.newArrayList();
    File solutionDir = getVSSolution().getSolutionDir();
    String workDir = getMicrosoftWindowsEnvironment().getWorkingDirectory();
    String reportDefaultPath = workDir + "/" + GallioConstants.GALLIO_COVERAGE_REPORT_XML;
    if (MODE_REUSE_REPORT.equals(executionMode)) {
      String[] reportPath = configuration.getStringArray(GallioConstants.REPORTS_COVERAGE_PATH_KEY, reportDefaultPath);
      reportFiles = FileFinder.findFiles(getVSSolution(), solutionDir, reportPath);
      LOG.info("Reusing Gallio coverage reports: " + Joiner.on(" ; ").join(reportFiles));
    } else if ( !getMicrosoftWindowsEnvironment().isTestExecutionDone()) {
      // This means that we are not in REUSE or SKIP mode, but for some reasons execution has not been done => skip the analysis
      LOG.info("Coverage report analysis won't execute as Gallio was not executed.");
    } else if (configuration.getBoolean(GallioConstants.SAFE_MODE, false)){
      reportFiles = FileFinder.findFiles(getVSSolution(), workDir, "*." + GallioConstants.GALLIO_COVERAGE_REPORT_XML);
      LOG.info("(Safe mode) Parsing Gallio coverage reports: " + Joiner.on(" ; ").join(reportFiles));
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

  private void parseAndSaveCoverageResults(Project project, SensorContext context, Collection<File> reportFiles) {
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
        saveCoverageMeasures(context, fileCoverage, sonarFile);
        context.saveMeasure(sonarFile, getHitData(fileCoverage));
      }
    }
  }

  private void saveCoverageMeasures(SensorContext context, FileCoverage coverageData, Resource<?> resource) {
    double coverage = coverageData.getCoverage();

    context.saveMeasure(resource, TestMetrics.ELOC, (double) coverageData.getCountLines());
    context.saveMeasure(resource, CoreMetrics.LINES_TO_COVER, (double) coverageData.getCountLines());
    context.saveMeasure(resource, CoreMetrics.UNCOVERED_LINES, (double) coverageData.getCountLines() - coverageData.getCoveredLines());
    context.saveMeasure(resource, CoreMetrics.COVERAGE, convertPercentage(coverage));
    // LINE_COVERAGE & COVERAGE should not be the same: need to have BRANCH_COVERAGE
    context.saveMeasure(resource, CoreMetrics.LINE_COVERAGE, convertPercentage(coverage));
  }

  /*
   * Generates a measure that contains the visits of each line of the source file.
   */
  private Measure getHitData(FileCoverage coverable) {
    lineHitsBuilder.clear();
    Map<Integer, SourceLine> lines = coverable.getLines();
    for (SourceLine line : lines.values()) {
      int lineNumber = line.getLineNumber();
      int countVisits = line.getCountVisits();
      lineHitsBuilder.add(Integer.toString(lineNumber), countVisits);
    }
    return lineHitsBuilder.build().setPersistenceMode(PersistenceMode.DATABASE);
  }

  private double convertPercentage(Number percentage) {
    return ParsingUtils.scaleValue(percentage.doubleValue() * 100.0);
  }

}