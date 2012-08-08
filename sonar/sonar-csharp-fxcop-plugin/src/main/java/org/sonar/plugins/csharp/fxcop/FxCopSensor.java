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
import java.util.Collection;
import java.util.Collections;

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
import org.sonar.plugins.csharp.api.sensor.AbstractRuleBasedCSharpSensor;
import org.sonar.plugins.csharp.fxcop.profiles.FxCopProfileExporter;

import com.google.common.base.Joiner;

/**
 * Collects the FXCop reporting into sonar.
 */
public class FxCopSensor extends AbstractRuleBasedCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(FxCopSensor.class);

  private ProjectFileSystem fileSystem;
  private RulesProfile rulesProfile;
  private FxCopProfileExporter profileExporter;
  private FxCopResultParser fxCopResultParser;

  @DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
  public static class RegularFxCopSensor extends FxCopSensor {
    public RegularFxCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, FxCopProfileExporter.RegularFxCopProfileExporter profileExporter,
        FxCopResultParser fxCopResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, fxCopResultParser, configuration, microsoftWindowsEnvironment);
    }
  }
  
  @DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
  public static class UnitTestsFxCopSensor extends FxCopSensor {
    public UnitTestsFxCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, FxCopProfileExporter.UnitTestsFxCopProfileExporter profileExporter,
        FxCopResultParser fxCopResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, fxCopResultParser, configuration, microsoftWindowsEnvironment);
    }
    @Override
    protected boolean isTestSensor() {
      return true;
    }
  }
  
  @Override
  protected boolean isCilSensor() {
    return true;
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
      FxCopResultParser fxCopResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(configuration, rulesProfile, profileExporter, microsoftWindowsEnvironment, "FxCop", configuration.getString(FxCopConstants.MODE, ""));
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.fxCopResultParser = fxCopResultParser;

  }
  
  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {
    
    fxCopResultParser.setEncoding(fileSystem.getSourceCharset());

    final Collection<File> reportFiles;
    String reportDefaultPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + FxCopConstants.FXCOP_REPORT_XML;
    if (MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      String reportPath = configuration.getString(FxCopConstants.REPORTS_PATH_KEY, reportDefaultPath);
      VisualStudioSolution vsSolution = getVSSolution();
      VisualStudioProject vsProject = getVSProject(project);
      reportFiles = FileFinder.findFiles(vsSolution, vsProject, reportPath);
      LOG.info("Reusing FxCop reports: " + Joiner.on(" ").join(reportFiles));
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
    builder.setIgnoreGeneratedCode(configuration.getBoolean(FxCopConstants.IGNORE_GENERATED_CODE_KEY,
        FxCopConstants.IGNORE_GENERATED_CODE_DEFVALUE));
    builder.setBuildConfiguration(configuration.getString(CSharpConstants.BUILD_CONFIGURATION_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));
    builder.setBuildPlatform(configuration.getString(CSharpConstants.BUILD_PLATFORM_KEY,
        CSharpConstants.BUILD_PLATFORM_DEFVALUE));
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
