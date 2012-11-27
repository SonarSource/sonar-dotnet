/*
 * Sonar .NET Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps;

import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;

import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.ndeps.NDepsCommandBuilder;
import org.sonar.dotnet.tools.ndeps.NDepsException;
import org.sonar.dotnet.tools.ndeps.NDepsRunner;
import org.sonar.plugins.csharp.ndeps.results.NDepsResultParser;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.DotNetConstants;
import org.sonar.plugins.dotnet.api.sensor.AbstractRegularDotNetSensor;
import org.sonar.plugins.dotnet.api.utils.FileFinder;

import java.io.File;

@DependsUpon(DotNetConstants.CORE_PLUGIN_EXECUTED)
public class NDepsSensor extends AbstractRegularDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(NDepsSensor.class);

  private ProjectFileSystem fileSystem;

  private NDepsResultParser nDepsResultParser;

  private boolean testSensor;

  /**
   * Constructor
   */
  public NDepsSensor(ProjectFileSystem fileSystem, MicrosoftWindowsEnvironment microsoftWindowsEnvironment, DotNetConfiguration configuration,
      NDepsResultParser nDepsResultParser) {
    super(configuration, microsoftWindowsEnvironment, "NDeps", configuration.getString(NDepsConstants.MODE));
    this.nDepsResultParser = nDepsResultParser;
    this.fileSystem = fileSystem;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  protected boolean isCilSensor() {
    return true;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  protected boolean isTestSensor() {
    return testSensor;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public boolean shouldExecuteOnProject(Project project) {
    VisualStudioProject vsProject = getVSProject(project);
    VisualStudioSolution solution = getVSSolution();
    if (vsProject == null) {
      // we must be at solution level
      return false;
    }

    if (getAssemblyPatterns() != null) {
      // Ndeps can analyse only one assembly
      if (FileFinder.findFiles(solution, vsProject, getAssemblyPatterns()).size() > 1) {
        LOG.warn("Skipping NDeps analysis, because multiple assemblies match " + DotNetConstants.ASSEMBLIES_TO_SCAN_KEY);
        return false;
      }
    }

    testSensor = vsProject.isTest(); // HACK in order to execute the senor on all projects
    return super.shouldExecuteOnProject(project) && !vsProject.isWebProject();
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public String[] getSupportedLanguages() {
    return NDepsConstants.SUPPORTED_LANGUAGES;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    final File reportFile;
    File projectDir = project.getFileSystem().getBasedir();
    String workingDirectory = getMicrosoftWindowsEnvironment().getWorkingDirectory();
    String reportDefaultPath = workingDirectory + "/" + NDepsConstants.DEPENDENCYPARSER_REPORT_XML;

    if (MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      String reportPath = configuration.getString(NDepsConstants.REPORTS_PATH_KEY);
      if (StringUtils.isEmpty(reportPath)) {
        reportPath = reportDefaultPath;
      }
      reportFile = FileFinder.browse(projectDir, reportPath);
      LOG.info("Reusing NDeps report: " + reportFile);
    } else {
      // run NDeps
      try {
        File tempDir = new File(getVSSolution().getSolutionDir(), workingDirectory);
        NDepsRunner runner = NDepsRunner.create(configuration.getString(NDepsConstants.INSTALL_DIR_KEY), tempDir.getAbsolutePath());

        launchNDeps(project, runner);
      } catch (NDepsException e) {
        throw new SonarException("NDeps execution failed.", e);
      }
      reportFile = new File(projectDir, reportDefaultPath);
    }

    // and analyse results
    analyseResults(project, reportFile);
  }

  protected void analyseResults(Project project, final File reportFile) {
    if (reportFile.exists()) {
      LOG.debug("NDeps report found at location {}", reportFile);
      String scope = isTestProject(project) ? "test" : "compile";
      nDepsResultParser.parse(scope, reportFile);
    } else {
      LOG.warn("No NDeps report found for path {}", reportFile);
    }
  }

  protected void launchNDeps(Project project, NDepsRunner runner) throws NDepsException {
    NDepsCommandBuilder builder = runner.createCommandBuilder(getVSSolution(), getVSProject(project));
    builder.setReportFile(new File(fileSystem.getSonarWorkingDirectory(), NDepsConstants.DEPENDENCYPARSER_REPORT_XML));
    builder.setBuildConfiguration(configuration.getString(DotNetConstants.BUILD_CONFIGURATION_KEY));
    builder.setBuildPlatform(configuration.getString(DotNetConstants.BUILD_PLATFORM_KEY));
    builder.setAssembliesToScan(getAssemblyPatterns());

    runner.execute(builder, configuration.getInt(NDepsConstants.TIMEOUT_MINUTES_KEY));
  }

}
