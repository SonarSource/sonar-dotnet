/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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
import java.util.Optional;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

@ScannerSide
public abstract class AbstractConfiguration {
  private static final Logger LOG = Loggers.get(AbstractConfiguration.class);
  private static final String MSG_SUFFIX = "'. No protobuf files will be loaded for this project.";
  private static final String PROP_PREFIX = "sonar.";

  private final Configuration configuration;
  private final String languageKey;

  public AbstractConfiguration(Configuration configuration, String languageKey) {
    this.configuration = configuration;
    this.languageKey = languageKey;
  }

  /**
   * To support Scanner for MSBuild <= 3.0
   */
  private String getOldRoslynJsonReportPathProperty() {
    return PROP_PREFIX + languageKey + ".roslyn.reportFilePath";
  }

  /**
   * The new property is written by scanners, and might contain multiple paths.
   */
  private String getRoslynJsonReportPathProperty() {
    return PROP_PREFIX + languageKey + ".roslyn.reportFilePaths";
  }

  private String getAnalyzerWorkDirProperty() {
    return PROP_PREFIX + languageKey + ".analyzer.projectOutPaths";
  }

  private String getOldAnalyzerWorkDirProperty() {
    return PROP_PREFIX + languageKey + ".analyzer.projectOutPath";
  }

  private String getAnalyzerReportDir() {
    return "output-" + languageKey;
  }

  /**
   * Returns path of the directory containing all protobuf files, if:
   * - Property with it's location is defined
   * - The directory exists
   * - The directory contains at least one protobuf
   */
  public List<Path> protobufReportPaths() {
    return protobufReportPaths(false);
  }

  /**
   * See {@link #protobufReportPath}. This method won't log anything.
   */
  public List<Path> protobufReportPathsSilent() {
    return protobufReportPaths(true);
  }

  private List<Path> protobufReportPaths(boolean silent) {
    List<Path> analyzerWorkDirPaths = Arrays.asList(configuration.getStringArray(getAnalyzerWorkDirProperty()))
      .stream().map(Paths::get).collect(Collectors.toList());

    if (analyzerWorkDirPaths.isEmpty()) {
      Optional<String> oldValue = configuration.get(getOldAnalyzerWorkDirProperty());
      if (oldValue.isPresent()) {
        analyzerWorkDirPaths = Collections.singletonList(Paths.get(oldValue.get()));
      } else {
        return empty("Property missing: '" + getAnalyzerWorkDirProperty() + "'" + MSG_SUFFIX, silent);
      }
    }

    return analyzerWorkDirPaths.stream()
      .map(x -> x.resolve(getAnalyzerReportDir()))
      .filter(p -> AbstractConfiguration.validateOutputDir(p, silent))
      .collect(Collectors.toList());
  }

  private static boolean validateOutputDir(Path analyzerOutputDir, boolean silent) {
    try {
      if (!analyzerOutputDir.toFile().exists()) {
        ifNotSilent(silent, () -> LOG.warn("Analyzer working directory does not exist: " + analyzerOutputDir.toAbsolutePath() + MSG_SUFFIX));
        return false;
      }

      try (DirectoryStream<Path> files = Files.newDirectoryStream(analyzerOutputDir, protoFileFilter())) {
        long count = StreamSupport.stream(files.spliterator(), false).count();
        if (count == 0) {
          ifNotSilent(silent, () -> LOG.warn("Analyzer working directory contains no .pb file(s)" + MSG_SUFFIX));
          return false;
        }

        ifNotSilent(silent, () -> LOG.debug("Analyzer working directory contains " + count + " .pb file(s)"));
        return true;
      }
    } catch (IOException e) {
      throw new IllegalStateException("Could not check for .pb files in " + analyzerOutputDir.toAbsolutePath(), e);
    }
  }

  static void ifNotSilent(boolean silent, Runnable r) {
    if (!silent) {
      r.run();
    }
  }

  static DirectoryStream.Filter<Path> protoFileFilter() {
    return p -> p.getFileName().toString().toLowerCase().endsWith(".pb");
  }

  private static List<Path> empty(String msg, boolean silent) {
    ifNotSilent(silent, () -> LOG.warn(msg));
    return Collections.emptyList();
  }

  public List<Path> roslynReportPaths() {
    String[] strPaths = configuration.getStringArray(getRoslynJsonReportPathProperty());
    if (strPaths.length > 0) {
      return Arrays.asList(strPaths).stream()
        .map(Paths::get)
        .collect(Collectors.toList());
    } else {
      // fallback to old property
      Optional<Path> path = configuration.get(getOldRoslynJsonReportPathProperty()).map(Paths::get);
      if (!path.isPresent()) {
        LOG.warn("No roslyn issues report not found for this project.");
        return Collections.emptyList();
      } else {
        LOG.debug("Found Roslyn issues report");
        return Collections.singletonList(path.get());
      }
    }
  }
}
