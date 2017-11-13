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

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFileFilter;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.FileMetadataImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;

import java.nio.file.Path;
import java.util.HashSet;
import java.util.Set;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.FILEMETADATA_OUTPUT_PROTOBUF_NAME;

public class GeneratedFileFilter implements InputFileFilter {
  private static final Logger LOG = Loggers.get(GeneratedFileFilter.class);

  private final AbstractConfiguration config;
  private final ProtobufImporters protobufImporters;

  private boolean initialized;
  private Set<Path> generatedFilePaths = new HashSet<>();

  public GeneratedFileFilter(AbstractConfiguration config) {
    this(config, new ProtobufImporters());
  }

  GeneratedFileFilter(AbstractConfiguration config, ProtobufImporters protobufImporters) {
    this.config = config;
    this.protobufImporters = protobufImporters;
  }

  @Override
  public boolean accept(InputFile inputFile) {
    initOnce();

    boolean isGenerated = generatedFilePaths.contains(inputFile.path());
    if (isGenerated) {
      LOG.debug("Skipping auto generated file: {}", inputFile.path());
    }
    return !isGenerated;
  }

  /**
   * synchronized because InputFileFilter are executed in parallel
   */
  private synchronized void initOnce() {
    if (initialized) {
      return;
    }
    initialized = true;

    Path protobufPath = config
            .protobufReportPathFromScanner()
            .resolve(FILEMETADATA_OUTPUT_PROTOBUF_NAME);

    if (protobufPath.toFile().exists()) {
      FileMetadataImporter fileMetadataImporter = protobufImporters.fileMetadataImporter();

      fileMetadataImporter.accept(protobufPath);

      generatedFilePaths.addAll(fileMetadataImporter.getGeneratedFilePaths());
    }
  }
}
