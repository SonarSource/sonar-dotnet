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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.gallio.GallioCommandBuilder;
import org.sonar.dotnet.tools.gallio.GallioException;
import org.sonar.dotnet.tools.gallio.GallioRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;

/**
 * Executes Gallio only once in the Solution directory to generate test execution and coverage reports.
 */
@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
@DependedUpon(GallioConstants.BARRIER_GALLIO_EXECUTED)
public class GallioSensor extends AbstractCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(GallioSensor.class);

  private CSharpConfiguration configuration;
  private String executionMode;

  /**
   * Constructs a {@link GallioSensor}.
   * 
   * @param fileSystem
   * @param configuration
   * @param microsoftWindowsEnvironment
   */
  public GallioSensor(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment);
    this.configuration = configuration;
    this.executionMode = configuration.getString(GallioConstants.MODE, "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (GallioConstants.MODE_SKIP.equalsIgnoreCase(executionMode)) {
      LOG.info("Gallio won't execute as it is set to 'skip' mode.");
      return false;
    }
    if (GallioConstants.MODE_REUSE_REPORT.equals(executionMode)) {
      LOG.info("Gallio won't execute as it is set to 'reuseReport' mode.");
      return false;
    }
    if (getMicrosoftWindowsEnvironment().isTestExecutionDone()) {
      LOG.info("Gallio won't execute as test execution has already been done.");
      return false;
    }
    if (getMicrosoftWindowsEnvironment().getCurrentSolution() != null
        && getMicrosoftWindowsEnvironment().getCurrentSolution().getTestProjects().isEmpty()) {
      LOG.info("Gallio won't execute as there are no test projects.");
      return false;
    }

    return super.shouldExecuteOnProject(project);
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    try {
      // create runner
      File gallioInstallDir = new File(configuration.getString(GallioConstants.INSTALL_FOLDER_KEY, GallioConstants.INSTALL_FOLDER_DEFVALUE));
      File workDir = new File(getMicrosoftWindowsEnvironment().getCurrentSolution().getSolutionDir(), getMicrosoftWindowsEnvironment()
          .getWorkingDirectory());
      if ( !workDir.exists()) {
        workDir.mkdirs();
      }
      GallioRunner runner = GallioRunner.create(gallioInstallDir.getAbsolutePath(), workDir.getAbsolutePath(), true);
      GallioCommandBuilder builder = runner.createCommandBuilder(getMicrosoftWindowsEnvironment().getCurrentSolution());

      // Add info for Gallio execution
      builder.setReportFile(new File(workDir, GallioConstants.GALLIO_REPORT_XML));
      builder.setFilter(configuration.getString(GallioConstants.FILTER_KEY, GallioConstants.FILTER_DEFVALUE));
      
      builder.setGallioRunnerType(configuration.getString(GallioConstants.RUNNER_TYPE_KEY, null));
      
      // Add info for coverage execution
      builder.setCoverageReportFile(new File(workDir, GallioConstants.GALLIO_COVERAGE_REPORT_XML));
      builder.setCoverageTool(configuration.getString(GallioConstants.COVERAGE_TOOL_KEY, GallioConstants.COVERAGE_TOOL_DEFVALUE));
      builder.setCoverageExcludes(configuration
          .getString(GallioConstants.COVERAGE_EXCLUDES_KEY, GallioConstants.COVERAGE_EXCLUDES_DEFVALUE));
      builder.setPartCoverInstallDirectory(new File(configuration.getString(GallioConstants.PART_COVER_INSTALL_KEY,
          GallioConstants.PART_COVER_INSTALL_DEFVALUE)));
      builder.setBuildConfigurations(configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
          CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));

      // and execute finally
      runner.execute(builder, configuration.getInt(GallioConstants.TIMEOUT_MINUTES_KEY, GallioConstants.TIMEOUT_MINUTES_DEFVALUE));
    } catch (GallioException e) {
      throw new SonarException("Gallio execution failed.", e);
    }

    // tell that tests were executed so that no other project tries to launch them a second time
    getMicrosoftWindowsEnvironment().setTestExecutionDone();
  }

}