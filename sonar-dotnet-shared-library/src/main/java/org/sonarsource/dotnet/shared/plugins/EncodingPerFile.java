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

import org.sonar.api.CoreProperties;
import org.sonar.api.SonarQubeVersion;
import org.sonar.api.batch.BatchSide;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.utils.Version;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.EncodingImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;

import java.nio.charset.Charset;
import java.nio.file.Path;
import java.util.Map;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ENCODING_OUTPUT_PROTOBUF_NAME;

@BatchSide
@ScannerSide
public class EncodingPerFile {
  private static final Logger LOG = Loggers.get(EncodingPerFile.class);
  private static final Version INPUT_FILE_CHARSET = Version.create(6, 1);

  // Lazy initialized
  private Map<Path, Charset> roslynEncodingPerPath;

  private final ProjectDefinition projectDef;
  private final SonarQubeVersion sonarQubeVersion;

  public EncodingPerFile(ProjectDefinition projectDef, SonarQubeVersion sonarQubeVersion) {
    this.projectDef = projectDef;
    this.sonarQubeVersion = sonarQubeVersion;
  }

  void init(Path reportDir) {
    EncodingImporter encodingImporter = ProtobufImporters.encodingImporter();

    Path encodingReportProtobuf = reportDir.resolve(ENCODING_OUTPUT_PROTOBUF_NAME);
    if (encodingReportProtobuf.toFile().exists()) {
      encodingImporter.accept(encodingReportProtobuf);
    } else {
      LOG.warn("Protobuf file not found: {}", encodingReportProtobuf);
    }

    this.roslynEncodingPerPath = encodingImporter.getEncodingPerPath();
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

    Charset sqEncoding;
    if (sonarQubeVersion.isGreaterThanOrEqual(INPUT_FILE_CHARSET)) {
      sqEncoding = inputFile.charset();
    } else {
      // Prior to 6.1 there was only global module encoding
      // can't use FileSystem::encoding since it is not yet initialized
      String encoding = projectDef.properties().get(CoreProperties.ENCODING_PROPERTY);
      sqEncoding = encoding != null && encoding.length() > 0 ? Charset.forName(encoding) : Charset.defaultCharset();
    }

    boolean sameEncoding = sqEncoding.equals(roslynEncoding);
    if (!sameEncoding) {
      LOG.warn("Encoding detected by Roslyn and encoding used by SonarQube do not match for file {}. "
        + "SonarQube encoding is '{}', Roslyn encoding is '{}'. File will be skipped.",
        inputFilePath, sqEncoding, roslynEncoding);
    }
    return sameEncoding;
  }

}
