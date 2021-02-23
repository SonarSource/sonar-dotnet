/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins;

import java.util.Optional;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.config.Configuration;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.PROJECT_BASE_DIR_PROPERTY;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.PROJECT_KEY_PROPERTY;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.PROJECT_NAME_PROPERTY;

/**
 * This class is a non-global sensor used to count the type of files in the .NET projects (i.e. Scanner modules).
 * <p>
 * Why is this needed?
 * - the Scanner for MSBuild categorizes projects as MAIN or TEST (see https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects)
 * - in SQ / SC, users can specify which files should be considered as MAIN (sources) or TEST (test sources) (see https://docs.sonarqube.org/latest/project-administration/narrowing-the-focus/)
 * - the categorization is not obvious, so this additional information should help users debug when needed
 */
@ScannerSide
public class FileTypeSensor implements Sensor {
  private static final Logger LOG = Loggers.get(FileTypeSensor.class);

  private final ProjectTypeCollector projectTypeCollector;
  private final DotNetPluginMetadata pluginMetadata;

  public FileTypeSensor(ProjectTypeCollector projectTypeCollector, DotNetPluginMetadata pluginMetadata) {
    this.projectTypeCollector = projectTypeCollector;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Project Type Information", pluginMetadata.shortLanguageName());
    descriptor.name(name);
    // we do not filter by language because we want to be called on projects without sources
    // (that could reference only shared sources e.g. in .projitems)
  }

  @Override
  public void execute(SensorContext context) {
    FileSystem fs = context.fileSystem();
    Configuration configuration = context.config();

    boolean hasMainFiles = SensorContextUtils.hasFilesOfType(fs, Type.MAIN, pluginMetadata.languageKey());
    boolean hasTestFiles = SensorContextUtils.hasFilesOfType(fs, Type.TEST, pluginMetadata.languageKey());

    Optional<String> analyzerWorkDir = getAnalyzerWorkDir(configuration);
    // We filter based on the "analyzerWorkDir" to avoid adding the top-level module, which has no files at all (is an artificial module with no MSBuild project equivalent).
    // The top-level module has the `sonar.projectKey` and `sonar.projectName` properties, but does not have the "analyzerWorkDir" property.
    if (analyzerWorkDir.isPresent()) {
      LOG.debug("Adding file type information (has MAIN '{}', has TEST '{}') for project '{}' (project key '{}', base dir '{}'). For debug info, see ProjectInfo.xml in '{}'.",
        hasMainFiles, hasTestFiles, getValueOrEmpty(configuration, PROJECT_NAME_PROPERTY),
        getValueOrEmpty(configuration, PROJECT_KEY_PROPERTY), getValueOrEmpty(configuration, PROJECT_BASE_DIR_PROPERTY), analyzerWorkDir.get());
      projectTypeCollector.addProjectInfo(hasMainFiles, hasTestFiles);
    }
  }

  private Optional<String> getAnalyzerWorkDir(Configuration configuration) {
    String property = AbstractPropertyDefinitions.getAnalyzerWorkDirProperty(pluginMetadata.languageKey());
    return configuration.get(property);
  }

  private static String getValueOrEmpty(Configuration configuration, String key) {
    Optional<String> optional = configuration.get(key);
    return optional.isPresent() ? optional.get() : "";
  }
}
