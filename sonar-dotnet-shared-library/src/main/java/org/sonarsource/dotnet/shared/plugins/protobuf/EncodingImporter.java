/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

import java.nio.charset.Charset;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.EncodingInfo;

/**
  This class is responsible of reading/importing the type of encoding that was detected by Roslyn for each and every compiled (C# or VB.NET) file.
  The info about the encoding detected by Roslyn is really important because SonarQube auto-detects the file encoding (which could result in a different detected encoding) AND
  because SonarQube only supports 1 encoding for ALL files (while Roslyn supports 1 encoding per file).

  Note that we could have merged this class and the FileMetadataImporter into only 1 protobuf message because the encoding is one other file metadata.
 */
public class EncodingImporter extends RawProtobufImporter<EncodingInfo> {

  private static final Logger LOG = Loggers.get(EncodingImporter.class);

  private final Map<String, Charset> encodingPerPath = new HashMap<>();

  public EncodingImporter() {
    super(SonarAnalyzer.EncodingInfo.parser());
  }

  @Override
  void consume(EncodingInfo message) {
    String roslynEncoding = message.getEncoding();
    Charset charset = null;
    if (!roslynEncoding.isEmpty()) {
      try {
        charset = Charset.forName(roslynEncoding);
      } catch (Exception e) {
        LOG.warn(String.format("Unrecognized encoding %s for file %s", roslynEncoding, message.getFilePath()), e);
      }
    }
    encodingPerPath.put(message.getFilePath(), charset);
  }

  public Map<Path, Charset> getEncodingPerPath() {
    // stream collector can't handle null values
    HashMap<Path, Charset> map = new HashMap<>();
    for (Map.Entry<String, Charset> e : encodingPerPath.entrySet()) {
      map.put(Paths.get(e.getKey()), e.getValue());
    }
    return Collections.unmodifiableMap(map);
  }
}
