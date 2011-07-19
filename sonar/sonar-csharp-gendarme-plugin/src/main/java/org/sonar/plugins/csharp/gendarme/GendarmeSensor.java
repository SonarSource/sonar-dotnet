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

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Collection;

import org.apache.commons.io.IOUtils;
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
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractRegularCSharpSensor;
import org.sonar.plugins.csharp.gendarme.profiles.GendarmeProfileExporter;
import org.sonar.plugins.csharp.gendarme.results.GendarmeResultParser;

import com.google.common.collect.Lists;

/**
 * Collects the Gendarme reporting into sonar.
 */
@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
public class GendarmeSensor extends AbstractRegularCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(GendarmeSensor.class);

  private ProjectFileSystem fileSystem;
  private RulesProfile rulesProfile;
  private GendarmeProfileExporter profileExporter;
  private GendarmeResultParser gendarmeResultParser;
  private CSharpConfiguration configuration;
  private String executionMode;

  /**
   * Constructs a {@link GendarmeSensor}.
   * 
   * @param fileSystem
   * @param ruleFinder
   * @param gendarmeRunner
   * @param profileExporter
   * @param rulesProfile
   */
  public GendarmeSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, GendarmeProfileExporter profileExporter,
      GendarmeResultParser gendarmeResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment);
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.gendarmeResultParser = gendarmeResultParser;
    this.configuration = configuration;
    this.executionMode = configuration.getString(GendarmeConstants.MODE, "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    boolean skipMode = GendarmeConstants.MODE_SKIP.equalsIgnoreCase(executionMode);
    if (skipMode) {
      LOG.info("Gendarme plugin won't execute as it is set to 'skip' mode.");
    }
    return super.shouldExecuteOnProject(project) && !skipMode;
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {
    if (rulesProfile.getActiveRulesByRepository(GendarmeConstants.REPOSITORY_KEY).isEmpty()) {
      LOG.warn("/!\\ SKIP Gendarme analysis: no rule defined for Gendarme in the \"{}\" profil.", rulesProfile.getName());
      return;
    }

    gendarmeResultParser.setEncoding(fileSystem.getSourceCharset());

    if ( !GendarmeConstants.MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
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
    }

    // and analyse results
    Collection<File> reportFiles = getReportFilesList();
    analyseResults(reportFiles);
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
    GendarmeCommandBuilder builder = runner.createCommandBuilder(getVSProject(project));
    builder.setConfigFile(gendarmeConfigFile);
    builder.setReportFile(new File(fileSystem.getSonarWorkingDirectory(), GendarmeConstants.GENDARME_REPORT_XML));
    builder.setConfidence(configuration
        .getString(GendarmeConstants.GENDARME_CONFIDENCE_KEY, GendarmeConstants.GENDARME_CONFIDENCE_DEFVALUE));
    builder.setSeverity("all");
    builder.setSilverlightFolder(getMicrosoftWindowsEnvironment().getSilverlightDirectory());
    builder.setBuildConfigurations(configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));
    runner.execute(builder, configuration.getInt(GendarmeConstants.TIMEOUT_MINUTES_KEY, GendarmeConstants.TIMEOUT_MINUTES_DEFVALUE));
  }

  protected Collection<File> getReportFilesList() {
    Collection<File> reportFiles = Lists.newArrayList();
    if (GendarmeConstants.MODE_REUSE_REPORT.equalsIgnoreCase(executionMode)) {
      File targetDir = fileSystem.getBuildDir();
      String[] reportsPath = configuration.getStringArray(GendarmeConstants.REPORTS_PATH_KEY);
      for (int i = 0; i < reportsPath.length; i++) {
        reportFiles.add(new File(targetDir, reportsPath[i]));
      }
      if (reportFiles.isEmpty()) {
        LOG.warn("No report to analyse whereas Gendame runs in 'reuseReport' mode. Please specify at least on report to analyse.");
      }
    } else {
      File sonarDir = fileSystem.getSonarWorkingDirectory();
      reportFiles.add(new File(sonarDir, GendarmeConstants.GENDARME_REPORT_XML));
    }
    return reportFiles;
  }

  protected void analyseResults(Collection<File> reportFiles) {
    for (File reportFile : reportFiles) {
      if (reportFile.exists()) {
        LOG.debug("Gendarme report found at location {}", reportFile);
        gendarmeResultParser.parse(reportFile);
      } else {
        LOG.warn("No Gendarme report found for path {}", reportFile);
      }
    }
  }

}