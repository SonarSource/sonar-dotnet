/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.CopyPasteTokenInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.MetricsInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.SymbolReferenceInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.TokenTypeInfo;
import org.sonarsource.dotnet.shared.StringUtils;
import org.sonarsource.dotnet.shared.plugins.protobuf.CPDTokensImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.HighlightImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.MetricsImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.RawProtobufImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.SymbolRefsImporter;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

@ScannerSide
public class ProtobufDataImporter {
  public static final String CPDTOKENS_FILENAME = "token-cpd.pb";
  public static final String FILEMETADATA_FILENAME = "file-metadata.pb";
  public static final String HIGHLIGHT_FILENAME = "token-type.pb";
  public static final String LOG_FILENAME = "log.pb";
  public static final String TELEMETRY_FILENAME = "telemetry.pb";
  public static final String METHODDECLARATIONS_FILENAME = "test-method-declarations.pb";
  public static final String METRICS_FILENAME = "metrics.pb";
  public static final String SYMBOLREFS_FILENAME = "symrefs.pb";

  private static final Logger LOG = LoggerFactory.getLogger(ProtobufDataImporter.class);

  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;

  public ProtobufDataImporter(FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter) {
    this.fileLinesContextFactory = fileLinesContextFactory;
    this.noSonarFilter = noSonarFilter;
  }

  public void importResults(SensorContext context, List<Path> protobufReportsDirectories, UnaryOperator<String> toRealPath) {
    RawProtobufImporter<MetricsInfo> metricsImporter = new MetricsImporter(context, fileLinesContextFactory, noSonarFilter, toRealPath);
    RawProtobufImporter<TokenTypeInfo> highlightImporter = new HighlightImporter(context, toRealPath);
    RawProtobufImporter<SymbolReferenceInfo> symbolRefsImporter = new SymbolRefsImporter(context, toRealPath);
    RawProtobufImporter<CopyPasteTokenInfo> cpdTokensImporter = new CPDTokensImporter(context, toRealPath);

    for (Path protobufReportsDir : protobufReportsDirectories) {
      long protoFiles = countProtoFiles(protobufReportsDir);
      LOG.info("Importing results from {} proto {} in '{}'",
        protoFiles,
        lazy(() -> StringUtils.pluralize("file", protoFiles)),
        protobufReportsDir);
      // Note: the no-sonar "measure" must be imported before issues, otherwise the affected issues won't get excluded!
      parseProtobuf(metricsImporter, protobufReportsDir, METRICS_FILENAME);
      parseProtobuf(highlightImporter, protobufReportsDir, HIGHLIGHT_FILENAME);
      parseProtobuf(symbolRefsImporter, protobufReportsDir, SYMBOLREFS_FILENAME);
      parseProtobuf(cpdTokensImporter, protobufReportsDir, CPDTOKENS_FILENAME);
    }

    metricsImporter.save();
    highlightImporter.save();
    symbolRefsImporter.save();
    cpdTokensImporter.save();
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

  public static void parseProtobuf(RawProtobufImporter<?> importer, Path workDirectory, String filename) {
    Path protobuf = workDirectory.resolve(filename);
    if (protobuf.toFile().exists()) {
      importer.accept(protobuf);
    } else {
      LOG.warn("Protobuf file not found: {}", protobuf);
    }
  }
}
