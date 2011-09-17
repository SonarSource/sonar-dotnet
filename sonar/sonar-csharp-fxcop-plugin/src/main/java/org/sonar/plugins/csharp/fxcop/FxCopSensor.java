/*
 * Sonar C# Plugin :: FxCop
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

package org.sonar.plugins.csharp.fxcop;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;

import org.apache.commons.io.IOUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.dotnet.tools.fxcop.FxCopCommandBuilder;
import org.sonar.dotnet.tools.fxcop.FxCopException;
import org.sonar.dotnet.tools.fxcop.FxCopRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCilRuleBasedCSharpSensor;
import org.sonar.plugins.csharp.fxcop.profiles.FxCopProfileExporter;


/**
 * Collects the FXCop reporting into sonar.
 */
@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
public class FxCopSensor extends AbstractCilRuleBasedCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(FxCopSensor.class);

  private ProjectFileSystem fileSystem;
  private RulesProfile rulesProfile;
  private FxCopProfileExporter profileExporter;
  private FxCopResultParser fxCopResultParser;
  private String executionMode;

  /**
   * Constructs a {@link FxCopSensor}.
   * 
   * @param fileSystem
   * @param ruleFinder
   * @param fxCopRunner
   * @param profileExporter
   * @param rulesProfile
   */
  public FxCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, FxCopProfileExporter profileExporter,
      FxCopResultParser fxCopResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment, configuration, "FxCop");
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.fxCopResultParser = fxCopResultParser;
    this.executionMode = configuration.getString(FxCopConstants.MODE, "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    
    boolean skipMode = FxCopConstants.MODE_SKIP.equalsIgnoreCase(executionMode);
    boolean reuseMode = FxCopConstants.MODE_REUSE_REPORT.equalsIgnoreCase(executionMode);
    if (skipMode) {
      LOG.info("FxCop plugin won't execute as it is set to 'skip' mode.");
    }
    return reuseMode || (super.shouldExecuteOnProject(project) && !skipMode);
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {
    if (rulesProfile.getActiveRulesByRepository(FxCopConstants.REPOSITORY_KEY).isEmpty()) {
      LOG.warn("/!\\ SKIP FxCop analysis: no rule defined for FxCop in the \"{}\" profil.", rulesProfile.getName());
      return;
    }

    fxCopResultParser.setEncoding(fileSystem.getSourceCharset());
    final File reportFile;
    File sonarDir = fileSystem.getSonarWorkingDirectory();
    if (FxCopConstants.MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      String reportPath 
        = configuration.getString(FxCopConstants.REPORTS_PATH_KEY, FxCopConstants.FXCOP_REPORT_XML);
      reportFile = FileFinder.browse(sonarDir, reportPath);
    } else {
      // prepare config file for FxCop
      File fxCopConfigFile = generateConfigurationFile();
      // and run FxCop
      try {
        FxCopRunner runner = FxCopRunner.create(configuration
            .getString(FxCopConstants.INSTALL_DIR_KEY, FxCopConstants.INSTALL_DIR_DEFVALUE));
        launchFxCop(project, runner, fxCopConfigFile);
      } catch (FxCopException e) {
        throw new SonarException("FxCop execution failed.", e);
      }
      reportFile = new File(sonarDir, FxCopConstants.FXCOP_REPORT_XML);
    }

    // and analyse results
    analyseResults(reportFile);
  }

  protected File generateConfigurationFile() {
    File configFile = new File(fileSystem.getSonarWorkingDirectory(), FxCopConstants.FXCOP_RULES_FILE);
    FileWriter writer = null;
    try {
      writer = new FileWriter(configFile);
      profileExporter.exportProfile(rulesProfile, writer);
      writer.flush();
    } catch (IOException e) {
      throw new SonarException("Error while generating the FxCop configuration file by exporting the Sonar rules.", e);
    } finally {
      IOUtils.closeQuietly(writer);
    }
    return configFile;
  }

  protected void launchFxCop(Project project, FxCopRunner runner, File fxCopConfigFile) throws FxCopException {
    VisualStudioSolution vsSolution = getVSSolution();
    VisualStudioProject vsProject = getVSProject(project);
    FxCopCommandBuilder builder = runner.createCommandBuilder(vsSolution, vsProject);
    builder.setConfigFile(fxCopConfigFile);
    builder.setReportFile(new File(fileSystem.getSonarWorkingDirectory(), FxCopConstants.FXCOP_REPORT_XML));
    builder.setAssembliesToScan(configuration.getStringArray(CSharpConstants.ASSEMBLIES_TO_SCAN_KEY));
    builder.setAssemblyDependencyDirectories(configuration.getStringArray(FxCopConstants.ASSEMBLY_DEPENDENCY_DIRECTORIES_KEY));
    builder.setIgnoreGeneratedCode(configuration.getBoolean(FxCopConstants.IGNORE_GENERATED_CODE_KEY,
        FxCopConstants.IGNORE_GENERATED_CODE_DEFVALUE));
    builder.setBuildConfigurations(configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));
    int timeout = configuration.getInt(FxCopConstants.TIMEOUT_MINUTES_KEY, FxCopConstants.TIMEOUT_MINUTES_DEFVALUE);
    builder.setTimeoutMinutes(timeout);
    builder.setSilverlightFolder(getMicrosoftWindowsEnvironment().getSilverlightDirectory());
    runner.execute(builder, timeout);
  }

  private void analyseResults(File reportFile) {
    if (reportFile.exists()) {
      LOG.debug("FxCop report found at location {}", reportFile);
      fxCopResultParser.parse(reportFile);
    } else {
      LOG.warn("No FxCop report found for path {}", reportFile);
    }
  }

}