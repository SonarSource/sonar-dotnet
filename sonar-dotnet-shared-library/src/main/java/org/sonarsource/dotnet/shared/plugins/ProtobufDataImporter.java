/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Locale;
import java.util.function.UnaryOperator;
import java.util.stream.Stream;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.CopyPasteTokenInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.MetricsInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.SymbolReferenceInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.TokenTypeInfo;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;
import org.sonarsource.dotnet.shared.plugins.protobuf.RawProtobufImporter;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.CPDTOKENS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;
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

  public void importResults(SensorContext context, List<Path> protobufReportsDirectories,
    UnaryOperator<String> toRealPath) {
    RawProtobufImporter<MetricsInfo> metricsImporter = ProtobufImporters.metricsImporter(context,
      fileLinesContextFactory, noSonarFilter, toRealPath);
    RawProtobufImporter<TokenTypeInfo> highlightImporter = ProtobufImporters.highlightImporter(context, toRealPath);
    RawProtobufImporter<SymbolReferenceInfo> symbolRefsImporter = ProtobufImporters.symbolRefsImporter(context, toRealPath);
    RawProtobufImporter<CopyPasteTokenInfo> cpdTokensImporter = ProtobufImporters.cpdTokensImporter(context, toRealPath);

    for (Path protobufReportsDir : protobufReportsDirectories) {
      long protoFiles = countProtoFiles(protobufReportsDir);
      LOG.info(String.format("Importing results from %d proto %s in '%s'", protoFiles, pluralize("file", protoFiles), protobufReportsDir));
      // Note: the no-sonar "measure" must be imported before issues, otherwise the affected issues won't get excluded!
      parseProtobuf(metricsImporter, protobufReportsDir, METRICS_OUTPUT_PROTOBUF_NAME);
      parseProtobuf(highlightImporter, protobufReportsDir, HIGHLIGHT_OUTPUT_PROTOBUF_NAME);
      parseProtobuf(symbolRefsImporter, protobufReportsDir, SYMBOLREFS_OUTPUT_PROTOBUF_NAME);
      parseProtobuf(cpdTokensImporter, protobufReportsDir, CPDTOKENS_OUTPUT_PROTOBUF_NAME);
    }

    metricsImporter.save();
    highlightImporter.save();
    symbolRefsImporter.save();
    cpdTokensImporter.save();
  }

  private static String pluralize(String s, long count) {
    if (count == 1) {
      return s;
    }
    return s + "s";
  }

  private static long countProtoFiles(Path dir) {
    try (Stream<Path> stream = Files.list(dir)) {
      return stream
        .filter(p -> p.getFileName().toString().toLowerCase(Locale.ENGLISH).endsWith(".pb"))
        .count();
    } catch (IOException e) {
      throw new IllegalStateException("unexpected error while reading files in: " + dir, e);
    }
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
