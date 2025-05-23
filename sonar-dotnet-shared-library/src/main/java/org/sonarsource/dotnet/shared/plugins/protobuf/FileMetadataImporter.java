/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import com.google.protobuf.Parser;
import java.net.URI;
import java.nio.charset.Charset;
import java.nio.file.Paths;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.FileMetadataInfo;

/**
  This class is responsible of reading/importing the result of the analyzer detection of whether a file is considered as auto-generated or not.
 */
public class FileMetadataImporter extends RawProtobufImporter<FileMetadataInfo> {

  private static final Logger LOG = LoggerFactory.getLogger(FileMetadataImporter.class);

  private final Map<URI, Charset> encodingPerUri = new HashMap<>();
  private final Set<URI> generatedFileUris = new HashSet<>();

  // For testing
  FileMetadataImporter(Parser<FileMetadataInfo> parser) {
    super(parser);
  }

  public FileMetadataImporter() {
    this(FileMetadataInfo.parser());
  }

  @Override
  void consume(FileMetadataInfo message) {
    URI fileUri = Paths.get(message.getFilePath()).toUri();
    if (message.getIsGenerated()) {
      generatedFileUris.add(fileUri);
    }
    String roslynEncoding = message.getEncoding();
    Charset charset = null;
    if (!roslynEncoding.isEmpty()) {
      try {
        charset = Charset.forName(roslynEncoding);
      } catch (Exception e) {
        LOG.warn(String.format("Unrecognized encoding %s for file %s", roslynEncoding, message.getFilePath()), e);
      }
    }
    encodingPerUri.put(fileUri, charset);
  }

  public Map<URI, Charset> getEncodingPerUri() {
    return Collections.unmodifiableMap(encodingPerUri);
  }

  public Set<URI> getGeneratedFileUris() {
    return generatedFileUris;
  }
}
