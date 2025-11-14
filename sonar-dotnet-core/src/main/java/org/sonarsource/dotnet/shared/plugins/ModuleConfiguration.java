/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.List;
import java.util.Locale;
import java.util.stream.StreamSupport;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.InstantiationStrategy;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.analyzerWorkDirProperty;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.roslynJsonReportPathProperty;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.telemetryJsonReportPathProperty;

/**
 * This configuration is at the level of the project ("module" in scanner-cli terminology).
 * <p>
 * Note: even if the concept of "module" was dropped from the SQ server side,
 * "modules" are still a core concept of the SQ scanner.
 * <p>
 * Module-independent configuration is in {@link AbstractLanguageConfiguration}.
 * <p>
 * Although module support has been dropped in SQ/SC, inside the scanner there is no good replacement for
 * {@link org.sonar.api.batch.ScannerSide}, yet.
 * When a replacement will appear, this code will have to be refactored.
 */
@ScannerSide
@InstantiationStrategy(InstantiationStrategy.PER_PROJECT)
public class ModuleConfiguration {
  public static final String TELEMETRY_JSON = "Telemetry.json";
  private static final Logger LOG = LoggerFactory.getLogger(ModuleConfiguration.class);
  private static final String MSG_SUFFIX = "Analyzer results won't be loaded from this directory.";

  private final Configuration configuration;
  private final String projectKey;
  private final PluginMetadata metadata;

  public ModuleConfiguration(Configuration configuration, PluginMetadata metadata) {
    this.configuration = configuration;
    this.metadata = metadata;
    this.projectKey = configuration.get(AbstractPropertyDefinitions.PROJECT_KEY_PROPERTY).orElse("<NONE>");
    LOG.trace("Project '{}': AbstractModuleConfiguration has been created.", projectKey);
  }

  static String getAnalyzerReportDir(String languageKey) {
    return "output-" + languageKey;
  }

  /**
   * Returns path of the directory containing all protobuf files, if:
   * - Property with it's location is defined
   * - The directory exists
   * - The directory contains at least one protobuf
   */
  public List<Path> protobufReportPaths() {
    List<Path> analyzerWorkDirPaths = Arrays.stream(configuration.getStringArray(analyzerWorkDirProperty(metadata.languageKey())))
      .map(Paths::get)
      .toList();

    if (analyzerWorkDirPaths.isEmpty() && !configuration.hasKey("sonar.tests")) {
      LOG.debug("Project '{}': Property missing: '{}'. No protobuf files will be loaded for this project.", projectKey,
        lazy(() -> analyzerWorkDirProperty(metadata.languageKey())));
    }

    return analyzerWorkDirPaths.stream().map(x -> x.resolve(getAnalyzerReportDir(metadata.languageKey())))
      .filter(this::validateOutputDir)
      .toList();
  }

  public List<Path> roslynReportPaths() {
    String[] strPaths = configuration.getStringArray(roslynJsonReportPathProperty(metadata.languageKey()));
    if (strPaths.length > 0) {
      LOG.debug("Project '{}': The Roslyn JSON report path has '{}'", projectKey, lazy(() -> String.join(",", strPaths)));
      return Arrays.stream(strPaths)
        .map(Paths::get)
        .toList();
    } else {
      LOG.debug("Project '{}': No Roslyn issues reports have been found.", projectKey);
      return Collections.emptyList();
    }
  }

  public Collection<Path> telemetryJsonPaths() {
    var telemetryJson = configuration.getStringArray(telemetryJsonReportPathProperty(metadata.languageKey()));
    return Arrays.stream(telemetryJson)
      .map(Paths::get)
      .filter(Files::exists)
      .toList();
  }

  private boolean validateOutputDir(Path analyzerOutputDir) {
    String path = analyzerOutputDir.toString();
    try {
      if (!analyzerOutputDir.toFile().exists()) {
        LOG.debug("Project '{}': Analyzer working directory does not exist: '{}'. {}", projectKey, path, MSG_SUFFIX);
        return false;
      }

      try (DirectoryStream<Path> files = Files.newDirectoryStream(analyzerOutputDir, protoFileFilter())) {
        long count = StreamSupport.stream(files.spliterator(), false).count();
        if (count == 0) {
          LOG.debug("Project '{}': Analyzer working directory '{}' contains no .pb file(s). {}", projectKey, path, MSG_SUFFIX);
          return false;
        }

        LOG.debug("Project '{}': Analyzer working directory '{}' contains {} .pb file(s)", projectKey, path, count);
        return true;
      }
    } catch (IOException e) {
      throw new IllegalStateException("Could not check for .pb files in '" + path + "' for project " + projectKey, e);
    }
  }

  private static DirectoryStream.Filter<Path> protoFileFilter() {
    return p -> p.getFileName().toString().toLowerCase(Locale.ROOT).endsWith(".pb");
  }
}
