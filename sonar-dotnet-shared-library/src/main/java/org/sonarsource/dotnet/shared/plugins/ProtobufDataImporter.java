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

import java.nio.file.Path;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;
import org.sonarsource.dotnet.shared.plugins.protobuf.RawProtobufImporter;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.CPDTOKENS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ISSUES_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.METRICS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.SYMBOLREFS_OUTPUT_PROTOBUF_NAME;

@ScannerSide
public class ProtobufDataImporter {
  private static final Logger LOG = Loggers.get(ProtobufDataImporter.class);

  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;

  public ProtobufDataImporter(FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter) {
    this.fileLinesContextFactory = fileLinesContextFactory;
    this.noSonarFilter = noSonarFilter;
  }

  public void importResults(SensorContext context, Path protobufReportsDirectory, String repositoryKey, boolean importIssues) {
    // Note: the no-sonar "measure" must be imported before issues, otherwise the affected issues won't get excluded!
    parseProtobuf(ProtobufImporters.metricsImporter(context, fileLinesContextFactory, noSonarFilter), protobufReportsDirectory, METRICS_OUTPUT_PROTOBUF_NAME);
    if (importIssues) {
      parseProtobuf(ProtobufImporters.issuesImporter(context, repositoryKey), protobufReportsDirectory, ISSUES_OUTPUT_PROTOBUF_NAME);
    }
    parseProtobuf(ProtobufImporters.highlightImporter(context), protobufReportsDirectory, HIGHLIGHT_OUTPUT_PROTOBUF_NAME);
    parseProtobuf(ProtobufImporters.symbolRefsImporter(context), protobufReportsDirectory, SYMBOLREFS_OUTPUT_PROTOBUF_NAME);
    parseProtobuf(ProtobufImporters.cpdTokensImporter(context), protobufReportsDirectory, CPDTOKENS_OUTPUT_PROTOBUF_NAME);
  }

  private static void parseProtobuf(RawProtobufImporter<?> importer, Path workDirectory, String filename) {
    Path protobuf = workDirectory.resolve(filename);
    if (protobuf.toFile().exists()) {
      importer.accept(protobuf);
    } else {
      LOG.warn("Protobuf file not found: " + protobuf);
    }
  }
}
