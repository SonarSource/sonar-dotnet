/*
 * Sonar C# Plugin :: NDeps
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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.ndeps.NDepsCommandBuilder;
import org.sonar.dotnet.tools.ndeps.NDepsException;
import org.sonar.dotnet.tools.ndeps.NDepsRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractRegularCSharpSensor;
import org.sonar.plugins.csharp.ndeps.results.NDepsResultParser;

import java.io.File;

@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
public class NDepsSensor extends AbstractRegularCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(NDepsSensor.class);

  private ProjectFileSystem fileSystem;

  private NDepsResultParser nDepsResultParser;

  private boolean testSensor;

  public NDepsSensor(ProjectFileSystem fileSystem, MicrosoftWindowsEnvironment microsoftWindowsEnvironment, CSharpConfiguration configuration,
      NDepsResultParser nDepsResultParser) {
    super(configuration, microsoftWindowsEnvironment, "NDeps", configuration.getString(NDepsConstants.MODE, ""));
    this.nDepsResultParser = nDepsResultParser;
    this.fileSystem = fileSystem;
  }

  @Override
  protected boolean isCilSensor() {
    return true;
  }

  @Override
  protected boolean isTestSensor() {
    return testSensor;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    VisualStudioProject vsProject = getVSProject(project);
    if (vsProject == null) {
      // we must be at solution level
      return false;
    }
    if (getAssemblyPatterns() != null && getAssemblyPatterns().length > 0) {
      LOG.warn("Skipping NDeps analysis, NDeps cannot work properly " +
        "when assembly locations are specified using " + CSharpConstants.ASSEMBLIES_TO_SCAN_KEY);
      return false;
    }

    testSensor = vsProject.isTest(); // HACK in order to execute the senor on all projects
    return super.shouldExecuteOnProject(project) && !vsProject.isWebProject();
  }

  public void analyse(Project project, SensorContext context) {
    final File reportFile;
    File projectDir = project.getFileSystem().getBasedir();
    String reportDefaultPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + NDepsConstants.DEPENDENCYPARSER_REPORT_XML;

    if (MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      String reportPath = configuration.getString(NDepsConstants.REPORTS_PATH_KEY, reportDefaultPath);
      reportFile = FileFinder.browse(projectDir, reportPath);
      LOG.info("Reusing NDeps report: " + reportFile);
    } else {
      // run NDeps
      try {
        File tempDir = new File(getMicrosoftWindowsEnvironment().getCurrentSolution().getSolutionDir(), getMicrosoftWindowsEnvironment()
            .getWorkingDirectory());
        NDepsRunner runner = NDepsRunner.create(
            configuration.getString(NDepsConstants.INSTALL_DIR_KEY, ""), tempDir.getAbsolutePath());

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
    builder.setBuildConfiguration(configuration.getString(CSharpConstants.BUILD_CONFIGURATION_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));
    builder.setBuildPlatform(configuration.getString(CSharpConstants.BUILD_PLATFORM_KEY,
        CSharpConstants.BUILD_PLATFORM_DEFVALUE));

    runner.execute(builder, configuration.getInt(NDepsConstants.TIMEOUT_MINUTES_KEY, NDepsConstants.TIMEOUT_MINUTES_DEFVALUE));
  }

}
