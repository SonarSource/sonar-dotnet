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
import java.util.Map;

import org.apache.commons.lang.StringUtils;
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
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractRegularCSharpSensor;
import org.sonar.plugins.csharp.gallio.results.coverage.CoverageResultParser;
import org.sonar.plugins.csharp.gallio.results.coverage.model.Coverable;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ParserResult;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ProjectCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.SourceLine;

/**
 * Gets the coverage test report and pushes data from it into sonar.
 */
@DependsUpon(GallioConstants.BARRIER_GALLIO_EXECUTED)
public class CoverageReportSensor extends AbstractRegularCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageReportSensor.class);

  private final PropertiesBuilder<String, Integer> lineHitsBuilder = new PropertiesBuilder<String, Integer>(
      CoreMetrics.COVERAGE_LINE_HITS_DATA);

  private CSharpConfiguration configuration;
  private String executionMode;

  /**
   * Constructs a {@link CoverageReportSensor}.
   * 
   * @param fileSystem
   * @param configuration
   * @param microsoftWindowsEnvironment
   */
  public CoverageReportSensor(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment);
    this.configuration = configuration;
    this.executionMode = configuration.getString(GallioConstants.MODE, "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    boolean skipMode = GallioConstants.MODE_SKIP.equalsIgnoreCase(executionMode);
    if (skipMode) {
      LOG.info("Coverage report analysis won't execute as it is set to 'skip' mode.");
    }

    return super.shouldExecuteOnProject(project) && !skipMode;
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    String reportPath = null;
    if (GallioConstants.MODE_REUSE_REPORT.equals(executionMode)) {
      reportPath = configuration.getString(GallioConstants.REPORTS_COVERAGE_PATH_KEY, "");
      LOG.info("Reusing Gallio coverage report: " + reportPath);
    } else {
      if ( !getMicrosoftWindowsEnvironment().isTestExecutionDone()) {
        // This means that we are not in REUSE or SKIP mode, but for some reasons execution has not been done => skip the analysis
        LOG.info("Coverage report analysis won't execute as Gallio was not executed.");
        return;
      }
      reportPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + GallioConstants.GALLIO_COVERAGE_REPORT_XML;
    }

    File reportFile = new File(getMicrosoftWindowsEnvironment().getCurrentSolution().getSolutionDir(), reportPath);
    if ( !reportFile.isFile()) {
      LOG.warn("No Gallio coverage report file found for: " + reportFile.getAbsolutePath());
      return;
    }

    // We parse the file and save the results
    parseAndSaveCoverageResults(project, context, reportFile);
  }

  protected void parseAndSaveCoverageResults(Project project, SensorContext context, File reportFile) {
    CoverageResultParser parser = new CoverageResultParser(context);
    ParserResult result = parser.parse(project, reportFile);
    
    VisualStudioSolution solution 
      = getMicrosoftWindowsEnvironment().getCurrentSolution();
    
    ProjectCoverage currentProjectCoverage = null;
    for (ProjectCoverage projectCoverage : result.getProjects()) {
      VisualStudioProject vsProject 
        = solution.getProject(projectCoverage.getAssemblyName());
      String branch = project.getBranch();
      final String vsProjectName;
      if (StringUtils.isEmpty(branch)) {
        vsProjectName = vsProject.getName(); 
      } else {
        vsProjectName = vsProject.getName() + " " +project.getBranch();
      }
      if (project.getName().equals(vsProjectName)) {
        currentProjectCoverage = projectCoverage;
        break;
      }
    }

    if (currentProjectCoverage != null) {
      // Save data for each file
      for (FileCoverage fileCoverage : currentProjectCoverage.getFileCoverageCollection()) {
        org.sonar.api.resources.File sonarFile = org.sonar.api.resources.File.fromIOFile(fileCoverage.getFile(), project);
        if (context.isIndexed(sonarFile, false)) {
          LOG.debug("- Saving coverage information for file {}", sonarFile.getKey());
          saveCoverageMeasures(context, fileCoverage, sonarFile);
          context.saveMeasure(sonarFile, getHitData(fileCoverage));
        }
      }
    }
  }

  protected void saveCoverageMeasures(SensorContext context, Coverable coverageData, Resource<?> resource) {
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
  protected Measure getHitData(FileCoverage coverable) {
    lineHitsBuilder.clear();
    Map<Integer, SourceLine> lines = coverable.getLines();
    for (SourceLine line : lines.values()) {
      int lineNumber = line.getLineNumber();
      int countVisits = line.getCountVisits();
      lineHitsBuilder.add(Integer.toString(lineNumber), countVisits);
    }
    return lineHitsBuilder.build().setPersistenceMode(PersistenceMode.DATABASE);
  }

  protected double convertPercentage(Number percentage) {
    return ParsingUtils.scaleValue(percentage.doubleValue() * 100.0);
  }

}