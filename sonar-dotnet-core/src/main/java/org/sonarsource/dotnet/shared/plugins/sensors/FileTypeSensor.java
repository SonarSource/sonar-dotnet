/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins.sensors;

import java.util.Optional;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.config.Configuration;
import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.ProjectTypeCollector;
import org.sonarsource.dotnet.shared.plugins.SensorContextUtils;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.PROJECT_BASE_DIR_PROPERTY;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.PROJECT_KEY_PROPERTY;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.PROJECT_NAME_PROPERTY;

/**
 * This class is a non-global sensor used to count the type of files in the .NET projects (i.e. Scanner modules).
 * <p>
 * Why is this needed?
 * - the Scanner for MSBuild categorizes projects as MAIN or TEST
 *   (see <a href="https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects">...</a>)
 * - in SQ / SC, users can specify which files should be considered as MAIN (sources) or TEST (test sources)
 *   (see <a href="https://docs.sonarqube.org/latest/project-administration/narrowing-the-focus/">...</a>)
 * - the categorization is not obvious, so this additional information should help users debug when needed
 */
@ScannerSide
public class FileTypeSensor implements Sensor {
  private static final Logger LOG = LoggerFactory.getLogger(FileTypeSensor.class);

  private final ProjectTypeCollector projectTypeCollector;
  private final PluginMetadata pluginMetadata;

  public FileTypeSensor(ProjectTypeCollector projectTypeCollector, PluginMetadata pluginMetadata) {
    this.projectTypeCollector = projectTypeCollector;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Project Type Information", pluginMetadata.languageName());
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
        hasMainFiles,
        hasTestFiles,
        lazy(() -> getValueOrEmpty(configuration, PROJECT_NAME_PROPERTY)),
        lazy(() -> getValueOrEmpty(configuration, PROJECT_KEY_PROPERTY)),
        lazy(() -> getValueOrEmpty(configuration, PROJECT_BASE_DIR_PROPERTY)),
        lazy(analyzerWorkDir::get));
      projectTypeCollector.addProjectInfo(hasMainFiles, hasTestFiles);
    }
  }

  private Optional<String> getAnalyzerWorkDir(Configuration configuration) {
    String property = AbstractPropertyDefinitions.getAnalyzerWorkDirProperty(pluginMetadata.languageKey());
    String[] values = configuration.getStringArray(property);
    if (values == null || values.length == 0) {
      return Optional.empty();
    }
    return Optional.of(String.join(", ", values));
  }

  private static String getValueOrEmpty(Configuration configuration, String key) {
    Optional<String> optional = configuration.get(key);
    return optional.isPresent() ? optional.get() : "";
  }
}
