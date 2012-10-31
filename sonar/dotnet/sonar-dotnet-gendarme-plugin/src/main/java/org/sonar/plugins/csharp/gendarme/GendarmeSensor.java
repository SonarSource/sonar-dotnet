/*
 * Sonar .NET Plugin :: Gendarme
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
package org.sonar.plugins.csharp.gendarme;

import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;

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
import org.sonar.dotnet.tools.gendarme.GendarmeCommandBuilder;
import org.sonar.dotnet.tools.gendarme.GendarmeException;
import org.sonar.dotnet.tools.gendarme.GendarmeRunner;
import org.sonar.plugins.csharp.gendarme.profiles.GendarmeProfileExporter;
import org.sonar.plugins.csharp.gendarme.results.GendarmeResultParser;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.DotNetConstants;
import org.sonar.plugins.dotnet.api.sensor.AbstractRuleBasedDotNetSensor;
import org.sonar.plugins.dotnet.api.utils.FileFinder;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Collection;
import java.util.Collections;

/**
 * Collects the Gendarme reporting into sonar.
 */
public abstract class GendarmeSensor extends AbstractRuleBasedDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(GendarmeSensor.class);

  private ProjectFileSystem fileSystem;
  private RulesProfile rulesProfile;
  private GendarmeProfileExporter profileExporter;
  private GendarmeResultParser gendarmeResultParser;

  @DependsUpon(DotNetConstants.CORE_PLUGIN_EXECUTED)
  public static class CSharpRegularGendarmeSensor extends GendarmeSensor {
    public CSharpRegularGendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter.CSharpRegularGendarmeProfileExporter profileExporter,
        GendarmeResultParser gendarmeResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, gendarmeResultParser, configuration, microsoftWindowsEnvironment);
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
  public static class VbNetRegularGendarmeSensor extends GendarmeSensor {
    public VbNetRegularGendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter.VbNetRegularGendarmeProfileExporter profileExporter,
        GendarmeResultParser gendarmeResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, gendarmeResultParser, configuration, microsoftWindowsEnvironment);
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String[] getSupportedLanguages() {
      return new String[] {"vbnet"};
    }
  }

  // Not used for the moment (see SONARPLUGINS-929)
  @DependsUpon(DotNetConstants.CORE_PLUGIN_EXECUTED)
  public static class UnitTestsGendarmeSensor extends GendarmeSensor {
    public UnitTestsGendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter.UnitTestsGendarmeProfileExporter profileExporter,
        GendarmeResultParser gendarmeResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, gendarmeResultParser, configuration, microsoftWindowsEnvironment);
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String[] getSupportedLanguages() {
      return new String[] {"cs"};
    }

    /**
     * {@inheritDoc}
     */
    @Override
    protected boolean isTestSensor() {
      return true;
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  protected boolean isCilSensor() {
    return true;
  }

  /**
   * Constructs a {@link GendarmeSensor}.
   * 
   * @param fileSystem
   * @param ruleFinder
   * @param gendarmeRunner
   * @param profileExporter
   * @param rulesProfile
   */
  protected GendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter profileExporter,
      GendarmeResultParser gendarmeResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(configuration, rulesProfile, profileExporter, microsoftWindowsEnvironment, "Gendarme", configuration.getString(GendarmeConstants.MODE));
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.gendarmeResultParser = gendarmeResultParser;
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {

    final Collection<File> reportFiles;
    String reportDefaultPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + GendarmeConstants.GENDARME_REPORT_XML;
    if (MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      String reportPath = configuration.getString(GendarmeConstants.REPORTS_PATH_KEY);
      if (StringUtils.isEmpty(reportPath)) {
        reportPath = reportDefaultPath;
      }
      VisualStudioSolution vsSolution = getVSSolution();
      VisualStudioProject vsProject = getVSProject(project);
      reportFiles = FileFinder.findFiles(vsSolution, vsProject, reportPath);
      LOG.info("Reusing Gendarme report: " + Joiner.on(" ").join(reportFiles));
    } else {
      // prepare config file for Gendarme
      File gendarmeConfigFile = generateConfigurationFile();
      // run Gendarme
      try {
        File tempDir = new File(getMicrosoftWindowsEnvironment().getCurrentSolution().getSolutionDir(), getMicrosoftWindowsEnvironment()
            .getWorkingDirectory());
        GendarmeRunner runner = GendarmeRunner.create(configuration.getString(GendarmeConstants.INSTALL_DIR_KEY), tempDir.getAbsolutePath());

        launchGendarme(project, runner, gendarmeConfigFile);
      } catch (GendarmeException e) {
        throw new SonarException("Gendarme execution failed.", e);
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
    File configFile = new File(fileSystem.getSonarWorkingDirectory(), GendarmeConstants.GENDARME_RULES_FILE);
    FileWriter writer = null;
    try {
      writer = new FileWriter(configFile);
      profileExporter.exportProfile(rulesProfile, writer);
      writer.flush();
    } catch (IOException e) {
      throw new SonarException("Error while generating the Gendarme configuration file by exporting the Sonar rules.", e);
    } finally {
      IOUtils.closeQuietly(writer);
    }
    return configFile;
  }

  protected void launchGendarme(Project project, GendarmeRunner runner, File gendarmeConfigFile) throws GendarmeException {
    GendarmeCommandBuilder builder = runner.createCommandBuilder(getVSSolution(), getVSProject(project));
    builder.setConfigFile(gendarmeConfigFile);
    builder.setReportFile(new File(fileSystem.getSonarWorkingDirectory(), GendarmeConstants.GENDARME_REPORT_XML));
    builder.setConfidence(configuration.getString(GendarmeConstants.GENDARME_CONFIDENCE_KEY));
    builder.setSeverity("all");
    builder.setSilverlightFolder(getMicrosoftWindowsEnvironment().getSilverlightDirectory());
    builder.setBuildConfiguration(configuration.getString(DotNetConstants.BUILD_CONFIGURATION_KEY));
    builder.setBuildPlatform(configuration.getString(DotNetConstants.BUILD_PLATFORM_KEY));

    String[] assemblies = configuration.getStringArray("sonar.gendarme.assemblies");
    if (assemblies == null || assemblies.length == 0) {
      assemblies = getAssemblyPatterns();
    } else {
      LOG.warn("Using deprecated key 'sonar.gendarme.assemblies', you should use instead " + DotNetConstants.ASSEMBLIES_TO_SCAN_KEY);
    }

    builder.setAssembliesToScan(assemblies);

    runner.execute(builder, configuration.getInt(GendarmeConstants.TIMEOUT_MINUTES_KEY));
  }

  private void analyseResults(File reportFile) {
    if (reportFile.exists()) {
      LOG.debug("Gendarme report found at location {}", reportFile);
      gendarmeResultParser.parse(reportFile);
    } else {
      LOG.warn("No Gendarme report found for path {}", reportFile);
    }
  }

}
