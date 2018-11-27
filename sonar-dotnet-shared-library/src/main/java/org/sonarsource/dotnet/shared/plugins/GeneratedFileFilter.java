/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2018 SonarSource SA
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

import java.nio.file.Path;
import java.util.Collections;
import java.util.List;
import java.util.Set;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFileFilter;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.FileMetadataImporter;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.FILEMETADATA_OUTPUT_PROTOBUF_NAME;

public class GeneratedFileFilter implements InputFileFilter {
  private static final Logger LOG = Loggers.get(GeneratedFileFilter.class);

  private Set<Path> generatedFilePaths = Collections.emptySet();

  public GeneratedFileFilter(AbstractConfiguration config) {
    List<Path> protobufPath = config.protobufReportPathsSilent();
    if (!protobufPath.isEmpty()) {
      Path metadataPath = protobufPath.get(0).resolve(FILEMETADATA_OUTPUT_PROTOBUF_NAME);
      if (metadataPath.toFile().exists()) {
        FileMetadataImporter fileMetadataImporter = new FileMetadataImporter();
        fileMetadataImporter.accept(metadataPath);
        generatedFilePaths = fileMetadataImporter.getGeneratedFilePaths();
      }
    }
  }

  @Override
  public boolean accept(InputFile inputFile) {
    boolean isGenerated = generatedFilePaths.contains(inputFile.path());
    if (isGenerated) {
      LOG.debug("Skipping auto generated file: {}", inputFile);
    }
    return !isGenerated;
  }

}
