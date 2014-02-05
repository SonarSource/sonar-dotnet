/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop;

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
import org.sonar.dotnet.tools.stylecop.StyleCopCommandBuilder;
import org.sonar.dotnet.tools.stylecop.StyleCopException;
import org.sonar.dotnet.tools.stylecop.StyleCopRunner;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.stylecop.profiles.StyleCopProfileExporter;
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
 * Collects the StyleCop reporting into sonar.
 */
public class StyleCopSensor extends AbstractRuleBasedDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(StyleCopSensor.class);
  private static final String[] SUPPORTED_LANGUAGES = new String[] {CSharpConstants.LANGUAGE_KEY};

  private final ProjectFileSystem fileSystem;
  private final RulesProfile rulesProfile;
  private final StyleCopProfileExporter profileExporter;
  private final StyleCopResultParser styleCopResultParser;
  private final DotNetConfiguration configuration;

  @DependsUpon(DotNetConstants.CORE_PLUGIN_EXECUTED)
  public static class RegularStyleCopSensor extends StyleCopSensor {

    public RegularStyleCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, StyleCopProfileExporter.RegularStyleCopProfileExporter profileExporter,
      StyleCopResultParser styleCopResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
      super(fileSystem, rulesProfile, profileExporter, styleCopResultParser, configuration, microsoftWindowsEnvironment);
    }
  }

  protected StyleCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, StyleCopProfileExporter profileExporter,
    StyleCopResultParser styleCopResultParser, DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(configuration, rulesProfile, profileExporter, microsoftWindowsEnvironment, "StyleCop", configuration.getString(StyleCopConstants.MODE));
    this.fileSystem = fileSystem;
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
    this.styleCopResultParser = styleCopResultParser;
    this.configuration = configuration;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public String[] getSupportedLanguages() {
    return SUPPORTED_LANGUAGES;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void analyse(Project project, SensorContext context) {

    final Collection<File> reportFiles;
    File projectDir = project.getFileSystem().getBasedir();
    String reportDefaultPath = getMicrosoftWindowsEnvironment().getWorkingDirectory() + "/" + StyleCopConstants.STYLECOP_REPORT_XML;
    if (MODE_REUSE_REPORT.equalsIgnoreCase(getExecutionMode())) {
      String reportPath = configuration.getString(StyleCopConstants.REPORTS_PATH_KEY);
      if (StringUtils.isBlank(reportPath)) {
        reportPath = reportDefaultPath;
      }
      VisualStudioSolution vsSolution = getVSSolution();
      VisualStudioProject vsProject = getVSProject(project);
      reportFiles = FileFinder.findFiles(vsSolution, vsProject, reportPath);
      LOG.info("Reusing StyleCop report: " + Joiner.on(" ").join(reportFiles));
    } else {
      // prepare config file for StyleCop
      File styleCopConfigFile = generateConfigurationFile(project);
      // run StyleCop
      try {
        File tempDir = new File(getMicrosoftWindowsEnvironment().getCurrentSolution().getSolutionDir(), getMicrosoftWindowsEnvironment()
          .getWorkingDirectory());
        StyleCopRunner runner = StyleCopRunner.create(
          configuration.getString(StyleCopConstants.INSTALL_DIR_KEY),
          getMicrosoftWindowsEnvironment().getDotnetSdkDirectory().getAbsolutePath(), tempDir.getAbsolutePath());
        launchStyleCop(project, runner, styleCopConfigFile);
      } catch (StyleCopException e) {
        throw new SonarException("StyleCop execution failed.", e);
      }
      reportFiles = Collections.singleton(new File(projectDir, reportDefaultPath));
    }

    // and analyse results
    for (File reportFile : reportFiles) {
      analyseResults(reportFile);
    }
  }

  protected void launchStyleCop(Project project, StyleCopRunner runner, File styleCopConfigFile) throws StyleCopException {
    StyleCopCommandBuilder builder = runner.createCommandBuilder(getMicrosoftWindowsEnvironment().getCurrentSolution(),
      getVSProject(project));
    builder.setConfigFile(styleCopConfigFile);
    builder.setReportFile(new File(fileSystem.getSonarWorkingDirectory(), StyleCopConstants.STYLECOP_REPORT_XML));
    runner.execute(builder, configuration.getInt(StyleCopConstants.TIMEOUT_MINUTES_KEY));
  }

  protected File generateConfigurationFile(Project project) {
    File configFile = new File(fileSystem.getSonarWorkingDirectory(), StyleCopConstants.STYLECOP_RULES_FILE);
    File analyzersSettings = findAnalyzersSettings(project);
    FileWriter writer = null;
    try {
      writer = new FileWriter(configFile);
      profileExporter.exportProfile(rulesProfile, writer, analyzersSettings);
      writer.flush();
    } catch (IOException e) {
      throw new SonarException("Error while generating the StyleCop configuration file by exporting the Sonar rules.", e);
    } finally {
      IOUtils.closeQuietly(writer);
    }
    return configFile;
  }

  private File findAnalyzersSettings(Project project) {
    String settingsPath = configuration.getString(StyleCopConstants.ANALYZERS_SETTINGS_PATH_KEY);
    if (StringUtils.isEmpty(settingsPath)) {
      return null;
    }

    Collection<File> settingsFiles = FileFinder.findFiles(getVSSolution(), getVSProject(project), settingsPath);
    final File result;
    if (settingsFiles.isEmpty()) {
      LOG.error("StyleCop analyzers settings not found");
      result = null;
    } else {
      if (settingsFiles.size() > 1) {
        LOG.warn("Multiple StyleCop analyzers settings found, only the first one will be used");
      }
      result = settingsFiles.iterator().next();
    }
    return result;
  }

  private void analyseResults(File reportFile) {
    if (reportFile.exists()) {
      LOG.debug("StyleCop report found at location {}", reportFile);
      styleCopResultParser.parse(reportFile);
    } else {
      LOG.warn("No StyleCop report found for path {}", reportFile);
    }
  }
}
