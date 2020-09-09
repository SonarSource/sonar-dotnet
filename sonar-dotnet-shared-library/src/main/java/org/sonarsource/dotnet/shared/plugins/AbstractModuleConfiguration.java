/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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

import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Locale;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;
import org.sonar.api.batch.InstantiationStrategy;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.getAnalyzerWorkDirProperty;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.getRoslynJsonReportPathProperty;

/**
 * This configuration is at the level of the project ("module" in scanner-cli terminology).
 *
 * Note: even if the concept of "module" was dropped from the SQ server side,
 * "modules" are still a core concept of the SQ scanner.
 *
 * Module-independent configuration is in {@link AbstractLanguageConfiguration}.
 *
 * @deprecated due to deprecation of module support and {@link org.sonar.api.batch.ScannerSide}.
 */
@ScannerSide
@InstantiationStrategy(InstantiationStrategy.PER_PROJECT)
@Deprecated
public abstract class AbstractModuleConfiguration {
  private static final Logger LOG = Loggers.get(AbstractModuleConfiguration.class);
  private static final String MSG_SUFFIX = "Analyzer results won't be loaded from this directory.";

  private final Configuration configuration;
  private final String languageKey;
  private final String projectKey;

  public AbstractModuleConfiguration(Configuration configuration, String languageKey) {
    this.configuration = configuration;
    this.languageKey = languageKey;
    this.projectKey = configuration.get("sonar.projectKey").orElse("<NONE>");
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
    List<Path> analyzerWorkDirPaths = Arrays.stream(configuration.getStringArray(getAnalyzerWorkDirProperty(languageKey)))
      .map(Paths::get)
      .collect(Collectors.toList());

    // we don't generate sonar.cs.analyzer.projectOutPaths for test projects on purpose
    if (analyzerWorkDirPaths.isEmpty() && !configuration.hasKey("sonar.tests")) {
      LOG.debug("Project '{}': Property missing: '{}'. No protobuf files will be loaded for this project.", projectKey, getAnalyzerWorkDirProperty(languageKey));
    }

    return analyzerWorkDirPaths.stream().map(x -> x.resolve(getAnalyzerReportDir(languageKey)))
      .filter(this::validateOutputDir)
      .collect(Collectors.toList());
  }

  public List<Path> roslynReportPaths() {
    String[] strPaths = configuration.getStringArray(getRoslynJsonReportPathProperty(languageKey));
    if (strPaths.length > 0) {
      LOG.debug("Project '{}': The Roslyn JSON report path has '{}'", projectKey, String.join(",", strPaths));
      return Arrays.stream(strPaths)
        .map(Paths::get)
        .collect(Collectors.toList());
    } else {
      LOG.debug( "Project '{}': No Roslyn issues reports have been found.", projectKey);
      return Collections.emptyList();
    }
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
