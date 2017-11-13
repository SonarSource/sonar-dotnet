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
import java.util.Optional;
import java.util.stream.StreamSupport;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

@ScannerSide
public abstract class AbstractConfiguration {

  private static final Logger LOG = Loggers.get(AbstractConfiguration.class);

  private final Configuration configuration;

  public AbstractConfiguration(Configuration configuration) {
    this.configuration = configuration;
  }

  protected abstract String getRoslynJsonReportPathProperty();

  protected abstract String getAnalyzerWorkDirProperty();

  protected abstract String getAnalyzerReportDir();

  /**
   * Returns path of the directory containing all protobuf files, if:
   * - Property with it's location is defined
   * - The directory exists
   * - The directory contains at least one protobuf
   */
  public Optional<Path> protobufReportPath() {
    Optional<String> analyzerWorkDirPath = configuration.get(getAnalyzerWorkDirProperty());

    if (!analyzerWorkDirPath.isPresent()) {
      LOG.warn("Property missing: '" + getAnalyzerWorkDirProperty() + "'. No protobuf files will be loaded.");
      return Optional.empty();
    }

    Path analyzerOutputDir = Paths.get(analyzerWorkDirPath.get(), getAnalyzerReportDir());
    configuration.get(getAnalyzerWorkDirProperty());

    if (!analyzerOutputDir.toFile().exists()) {
      LOG.warn("Analyzer working directory does not exist: " + analyzerOutputDir.toAbsolutePath() + ". No protobuf files will be loaded.");
      return Optional.empty();
    }

    try (DirectoryStream<Path> files = Files.newDirectoryStream(analyzerOutputDir, p -> p.getFileName().toString().toLowerCase().endsWith(".pb"))) {
      long count = StreamSupport.stream(files.spliterator(), false).count();
      if (count == 0) {
        LOG.warn("Analyzer working directory contains no .pb file(s). No protobuf files will be loaded.");
        return Optional.empty();
      }
      LOG.info("Analyzer working directory contains " + count + " .pb file(s)");

      return Optional.of(analyzerOutputDir);
    } catch (IOException e) {
      throw new IllegalStateException("Could not check for .pb files in " + analyzerOutputDir.toAbsolutePath(), e);
    }
  }

  public Optional<Path> roslynReportPath() {
    return configuration.get(getRoslynJsonReportPathProperty()).map(Paths::get);
  }
}
