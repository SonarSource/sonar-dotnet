/*
 * Sonar C# Plugin :: Gendarme
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

import com.google.common.base.Joiner;
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
import org.sonar.dotnet.tools.gendarme.GendarmeCommandBuilder;
import org.sonar.dotnet.tools.gendarme.GendarmeException;
import org.sonar.dotnet.tools.gendarme.GendarmeRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractRuleBasedCSharpSensor;
import org.sonar.plugins.csharp.gendarme.profiles.GendarmeProfileExporter;
import org.sonar.plugins.csharp.gendarme.results.GendarmeResultParser;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Collection;
import java.util.Collections;

/**
 * Collects the Gendarme reporting into sonar.
 */
public class GendarmeSensor extends AbstractRuleBasedCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(GendarmeSensor.class);

  private ProjectFileSystem fileSystem;
  private RulesProfile rulesProfile;
  private GendarmeProfileExporter profileExporter;
  private GendarmeResultParser gendarmeResultParser;

  @DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
  public static class RegularGendarmeSensor extends GendarmeSensor {
    public RegularGendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter.RegularGendarmeProfileExporter profileExporter,
        GendarmeResultParser gendarmeResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, gendarmeResultParser, configuration, microsoftWindowsEnvironment);
    }
  }

  @DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
  public static class UnitTestsGendarmeSensor extends GendarmeSensor {
    public UnitTestsGendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter.UnitTestsGendarmeProfileExporter profileExporter,
        GendarmeResultParser gendarmeResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, gendarmeResultParser, configuration, microsoftWindowsEnvironment);
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
   * Constructs a {@link GendarmeSensor}.
   * 
   * @param fileSystem
   * @param ruleFinder
   * @param gendarmeRunner
   * @param profileExporter
   * @param rulesProfile
   */
  protected GendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter profileExporter,
      GendarmeResultParser gendarmeResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(configuration, rulesProfile, profileExporter, microsoftWindowsEnvironment, "Gendarme", configuration.getString(GendarmeConstants.MODE, ""));
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.gendarmeResultParser = gendarmeResultParser;
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {

    gendarmeResultParser.setEncoding(fileSystem.getSourceCharset());
    final Collection<File> reportFiles;
    String reportDefaultPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + GendarmeConstants.GENDARME_REPORT_XML;
    if (MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      String reportPath = configuration.getString(GendarmeConstants.REPORTS_PATH_KEY, reportDefaultPath);
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
        GendarmeRunner runner = GendarmeRunner.create(
            configuration.getString(GendarmeConstants.INSTALL_DIR_KEY, GendarmeConstants.INSTALL_DIR_DEFVALUE), tempDir.getAbsolutePath());

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
    builder.setConfidence(configuration
        .getString(GendarmeConstants.GENDARME_CONFIDENCE_KEY, GendarmeConstants.GENDARME_CONFIDENCE_DEFVALUE));
    builder.setSeverity("all");
    builder.setSilverlightFolder(getMicrosoftWindowsEnvironment().getSilverlightDirectory());
    builder.setBuildConfiguration(configuration.getString(CSharpConstants.BUILD_CONFIGURATION_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));
    builder.setBuildPlatform(configuration.getString(CSharpConstants.BUILD_PLATFORM_KEY,
        CSharpConstants.BUILD_PLATFORM_DEFVALUE));

    String[] assemblies = configuration.getStringArray("sonar.gendarme.assemblies");
    if (assemblies == null || assemblies.length == 0) {
      assemblies = getAssemblyPatterns();
    } else {
      LOG.warn("Using deprecated key 'sonar.gendarme.assemblies', you should use instead " + CSharpConstants.ASSEMBLIES_TO_SCAN_KEY);
    }

    builder.setAssembliesToScan(assemblies);

    runner.execute(builder, configuration.getInt(GendarmeConstants.TIMEOUT_MINUTES_KEY, GendarmeConstants.TIMEOUT_MINUTES_DEFVALUE));
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
