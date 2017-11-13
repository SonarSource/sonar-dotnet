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

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.util.Collections;
import java.util.Map;
import java.util.Optional;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.EncodingImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ENCODING_OUTPUT_PROTOBUF_NAME;

@ScannerSide
public class EncodingPerFile {
  private static final Logger LOG = Loggers.get(EncodingPerFile.class);

  // Lazy initialized
  private Map<Path, Charset> roslynEncodingPerPath;

  void init(Optional<Path> protobufDir) {
    if (protobufDir.isPresent()) {
      Path encodingReportProtobuf = protobufDir.get().resolve(ENCODING_OUTPUT_PROTOBUF_NAME);
      if (encodingReportProtobuf.toFile().exists()) {
        EncodingImporter encodingImporter = ProtobufImporters.encodingImporter();
        encodingImporter.accept(encodingReportProtobuf);
        this.roslynEncodingPerPath = encodingImporter.getEncodingPerPath();
        return;
      }
    }

    this.roslynEncodingPerPath = Collections.emptyMap();
    LOG.warn("Protobuf file with encoding not found");

  }

  boolean encodingMatch(InputFile inputFile) {
    Path inputFilePath = inputFile.path().toAbsolutePath();

    if (!roslynEncodingPerPath.containsKey(inputFilePath)) {
      // When there is no entry for a file, it means it was not processed by Roslyn. So we consider encoding to be ok.
      return true;
    }

    Charset roslynEncoding = roslynEncodingPerPath.get(inputFilePath);
    if (roslynEncoding == null) {
      LOG.warn("File '{}' does not have encoding information. Skip it.", inputFilePath);
      return false;
    }

    Charset sqEncoding = inputFile.charset();

    boolean sameEncoding = sqEncoding.equals(roslynEncoding);
    if (!sameEncoding) {
      if (sqEncoding.equals(StandardCharsets.UTF_16LE) && roslynEncoding.equals(StandardCharsets.UTF_16)) {
        sameEncoding = true;
      } else {
        LOG.warn("Encoding detected by Roslyn and encoding used by SonarQube do not match for file {}. "
          + "SonarQube encoding is '{}', Roslyn encoding is '{}'. File will be skipped.",
          inputFilePath, sqEncoding, roslynEncoding);
      }
    }
    return sameEncoding;
  }
}
