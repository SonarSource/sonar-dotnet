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
import java.util.stream.StreamSupport;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Settings;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

@ScannerSide
public abstract class AbstractConfiguration {

  private static final Logger LOG = Loggers.get(AbstractConfiguration.class);

  private final Settings settings;

  public AbstractConfiguration(Settings settings) {
    this.settings = settings;
  }

  public abstract String getRoslynJsonReportPathProperty();

  public abstract String getAnalyzerWorkDirProperty();

  public abstract String getAnalyzerReportDir();

  public boolean areProtobufReportsPresent() {
    if (!settings.hasKey(getAnalyzerWorkDirProperty())) {
      LOG.warn("Property missing: '" + getAnalyzerWorkDirProperty() + "'. No protobuf files will be loaded.");
      return false;
    }
    Path analyzerOutputDir = protobufReportPathFromScanner();

    if (!analyzerOutputDir.toFile().exists()) {
      LOG.warn("Analyzer working directory does not exist: " + analyzerOutputDir.toAbsolutePath() + ". No protobuf files will be loaded.");
      return false;
    }

    try (DirectoryStream<Path> files = Files.newDirectoryStream(analyzerOutputDir, p -> p.getFileName().toString().toLowerCase().endsWith(".pb"))) {
      long count = StreamSupport.stream(files.spliterator(), false).count();
      if (count == 0) {
        LOG.warn("Analyzer working directory contains no .pb file(s). No protobuf files will be loaded.");
        return false;
      }
      LOG.info("Analyzer working directory contains " + count + " .pb file(s)");
      return true;
    } catch (IOException e) {
      throw new IllegalStateException("Could not check for .pb files in " + analyzerOutputDir.toAbsolutePath(), e);
    }
  }

  public Path protobufReportPathFromScanner() {
    String analyzerWorkDirPath = settings.getString(getAnalyzerWorkDirProperty());
    return Paths.get(analyzerWorkDirPath, getAnalyzerReportDir());
  }

}
