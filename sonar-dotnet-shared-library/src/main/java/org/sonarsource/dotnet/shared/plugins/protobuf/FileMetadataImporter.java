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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import com.google.protobuf.Parser;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.FileMetadataInfo;

import java.util.HashMap;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;

public class FileMetadataImporter extends RawProtobufImporter<FileMetadataInfo> {

  private final Map<String, FileMetadataInfo> fileMetadata = new HashMap<>();

  // For testing
  FileMetadataImporter(Parser<FileMetadataInfo> parser) {
    super(parser);
  }

  FileMetadataImporter() {
    this(FileMetadataInfo.parser());
  }

  @Override
  void consume(FileMetadataInfo message) {
    fileMetadata.put(message.getFilePath(), message);
  }

  public Set<String> getGeneratedFilePaths() {
    return fileMetadata.entrySet()
            .stream()
            .filter(entry -> entry.getValue().getIsGenerated())
            .map(entry -> entry.getValue().getFilePath())
            .collect(Collectors.toSet());
  }
}
