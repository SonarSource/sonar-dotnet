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

import org.sonar.api.batch.BatchSide;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Settings;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.stream.StreamSupport;

@BatchSide
@ScannerSide
public abstract class AbstractConfiguration {

  private static final Logger LOG = Loggers.get(AbstractConfiguration.class);

  private final Settings settings;
  private Boolean reportsComingFromMSBuild;

  public AbstractConfiguration(Settings settings) {
    this.settings = settings;
  }

  public abstract String getRoslynJsonReportPathProperty();

  public abstract String getAnalyzerWorkDirProperty();

  public abstract String getAnalyzerReportDir();

  public boolean isReportsComingFromMSBuild() {
    if (reportsComingFromMSBuild == null) {
      reportsComingFromMSBuild = areProtobufReportsPresent();
    }
    return reportsComingFromMSBuild;
  }

  private boolean areProtobufReportsPresent() {
    if (!settings.hasKey(getAnalyzerWorkDirProperty())) {
      return false;
    }
    Path analyzerOutputDir = protobufReportPathFromScanner();

    if (!analyzerOutputDir.toFile().exists()) {
      LOG.info("Analyzer working directory does not exist");
      return false;
    }

    try (DirectoryStream<Path> files = Files.newDirectoryStream(analyzerOutputDir, p -> p.toAbsolutePath().toString().toLowerCase().endsWith(".pb"))) {
      long count = StreamSupport.stream(files.spliterator(), false).count();
      LOG.info("Analyzer working directory contains " + count + " .pb file(s)");
      return count != 0;
    } catch (IOException e) {
      LOG.warn("Could not check for .pb files in " + analyzerOutputDir.toAbsolutePath().toString(), e);
      return false;
    }
  }

  public Path protobufReportPathFromScanner() {
    String analyzerWorkDirPath = settings.getString(getAnalyzerWorkDirProperty());
    return Paths.get(analyzerWorkDirPath, getAnalyzerReportDir());
  }

}
