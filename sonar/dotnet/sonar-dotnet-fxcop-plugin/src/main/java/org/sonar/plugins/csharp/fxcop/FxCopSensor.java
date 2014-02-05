/*
 * Sonar .NET Plugin :: FxCop
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

import com.google.common.base.Joiner;
import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.fxcop.FxCopCommandBuilder;
import org.sonar.dotnet.tools.fxcop.FxCopException;
import org.sonar.dotnet.tools.fxcop.FxCopRunner;
import org.sonar.plugins.csharp.fxcop.profiles.FxCopProfileExporter;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.DotNetConstants;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;
import org.sonar.plugins.dotnet.api.sensor.AbstractRuleBasedDotNetSensor;
import org.sonar.plugins.dotnet.api.utils.FileFinder;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Collection;
import java.util.Collections;

/**
 * Collects the FXCop reporting into sonar.
 */
public abstract class FxCopSensor extends AbstractRuleBasedDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(FxCopSensor.class);

  private final ProjectFileSystem fileSystem;
  private final RulesProfile rulesProfile;
  private final FxCopProfileExporter profileExporter;
  private final FxCopResultParser fxCopResultParser;

  @DependsUpon(DotNetConstants.CORE_PLUGIN_EXECUTED)
  public static class CSharpRegularFxCopSensor extends FxCopSensor {
    public CSharpRegularFxCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, FxCopProfileExporter.CSharpRegularFxCopProfileExporter profileExporter,
      FxCopResultParser fxCopResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, fxCopResultParser, configuration, microsoftWindowsEnvironment);
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String[] getSupportedLanguages() {
      return new String[] {"cs"};
    }
  }

  @DependsUpon(DotNetConstants.CORE_PLUGIN_EXECUTED)
  public static class VbNetRegularFxCopSensor extends FxCopSensor {
    public VbNetRegularFxCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, FxCopProfileExporter.VbNetRegularFxCopProfileExporter profileExporter,
      FxCopResultParser fxCopResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, fxCopResultParser, configuration, microsoftWindowsEnvironment);
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String[] getSupportedLanguages() {
      return new String[] {"vbnet"};
    }
  }

  /**
   * Constructs a {@link FxCopSensor}.
   *
   * @param fileSystem
   * @param ruleFinder
   * @param fxCopRunner
   * @param profileExporter
   * @param rulesProfile
   */
  protected FxCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, FxCopProfileExporter profileExporter,
    FxCopResultParser fxCopResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(configuration, rulesProfile, profileExporter, microsoftWindowsEnvironment, "FxCop", configuration.getString(FxCopConstants.MODE));
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.fxCopResultParser = fxCopResultParser;

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
  public void analyse(Project project, SensorContext context) {

    final Collection<File> reportFiles;
    String reportDefaultPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + FxCopConstants.FXCOP_REPORT_XML;
    if (MODE_REUSE_REPORT.equalsIgnoreCase(getExecutionMode())) {
      String reportPath = configuration.getString(FxCopConstants.REPORTS_PATH_KEY);
      if (StringUtils.isEmpty(reportPath)) {
        reportPath = reportDefaultPath;
      }
      VisualStudioSolution vsSolution = getVSSolution();
      VisualStudioProject vsProject = getVSProject(project);
      reportFiles = FileFinder.findFiles(vsSolution, vsProject, reportPath);
      LOG.info("Reusing FxCop reports: " + Joiner.on(" ").join(reportFiles));
    } else {
      // prepare config file for FxCop
      File fxCopConfigFile = generateConfigurationFile();
      // and run FxCop
      try {
        FxCopRunner runner = FxCopRunner.create(configuration.getString(FxCopConstants.INSTALL_DIR_KEY));
        launchFxCop(project, runner, fxCopConfigFile);
      } catch (FxCopException e) {
        throw new SonarException("FxCop execution failed.", e);
      }
      File projectDir = fileSystem.getBasedir();
      reportFiles = Collections.singleton(new File(projectDir, reportDefaultPath));
    }

    // and analyze results
    for (File reportFile : reportFiles) {
      analyseResults(reportFile);
    }
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

    builder.setAssembliesToScan(getAssemblyPatterns());

    builder.setAssemblyDependencyDirectories(configuration.getStringArray(FxCopConstants.ASSEMBLY_DEPENDENCY_DIRECTORIES_KEY));
    builder.setIgnoreGeneratedCode(configuration.getBoolean(FxCopConstants.IGNORE_GENERATED_CODE_KEY));
    builder.setBuildConfiguration(configuration.getString(DotNetConstants.BUILD_CONFIGURATION_KEY));
    builder.setBuildPlatform(configuration.getString(DotNetConstants.BUILD_PLATFORM_KEY));
    int timeout = configuration.getInt(FxCopConstants.TIMEOUT_MINUTES_KEY);
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
