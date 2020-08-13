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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.util.function.UnaryOperator;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.CopyPasteTokenInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.MetricsInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.SymbolReferenceInfo;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.TokenTypeInfo;

@ScannerSide
public class ProtobufImporters {

  private ProtobufImporters() {
    // utility class
  }

  public static final String HIGHLIGHT_OUTPUT_PROTOBUF_NAME = "token-type.pb";
  public static final String SYMBOLREFS_OUTPUT_PROTOBUF_NAME = "symrefs.pb";
  public static final String CPDTOKENS_OUTPUT_PROTOBUF_NAME = "token-cpd.pb";
  public static final String METRICS_OUTPUT_PROTOBUF_NAME = "metrics.pb";
  public static final String FILEMETADATA_OUTPUT_PROTOBUF_NAME = "file-metadata.pb";

  public static RawProtobufImporter<TokenTypeInfo> highlightImporter(SensorContext context,
    UnaryOperator<String> toRealPath) {
    return new HighlightImporter(context, toRealPath);
  }

  public static RawProtobufImporter<SymbolReferenceInfo> symbolRefsImporter(SensorContext context,
    UnaryOperator<String> toRealPath) {
    return new SymbolRefsImporter(context, toRealPath);
  }

  public static RawProtobufImporter<CopyPasteTokenInfo> cpdTokensImporter(SensorContext context,
    UnaryOperator<String> toRealPath) {
    return new CPDTokensImporter(context, toRealPath);
  }

  public static RawProtobufImporter<MetricsInfo> metricsImporter(SensorContext context,
    FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter, UnaryOperator<String> toRealPath) {
    return new MetricsImporter(context, fileLinesContextFactory, noSonarFilter, toRealPath);
  }

}
