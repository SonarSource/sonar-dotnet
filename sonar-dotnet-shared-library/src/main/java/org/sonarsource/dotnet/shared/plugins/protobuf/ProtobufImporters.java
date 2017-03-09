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

import java.util.function.Predicate;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;

public final class ProtobufImporters {

  public static final String HIGHLIGHT_OUTPUT_PROTOBUF_NAME = "token-type.pb";
  public static final String SYMBOLREFS_OUTPUT_PROTOBUF_NAME = "symbol-reference.pb";
  public static final String CPDTOKENS_OUTPUT_PROTOBUF_NAME = "token-cpd.pb";
  public static final String METRICS_OUTPUT_PROTOBUF_NAME = "metrics.pb";
  public static final String ISSUES_OUTPUT_PROTOBUF_NAME = "issues.pb";
  public static final String ENCODING_OUTPUT_PROTOBUF_NAME = "encoding.pb";

  private ProtobufImporters() {
    // utility class, forbidden constructor
  }

  public static RawProtobufImporter highlightImporter(SensorContext context, Predicate<InputFile> inputFileFilter) {
    return new HighlightImporter(context, inputFileFilter);
  }

  public static RawProtobufImporter symbolRefsImporter(SensorContext context, Predicate<InputFile> inputFileFilter) {
    return new SymbolRefsImporter(context, inputFileFilter);
  }

  public static RawProtobufImporter cpdTokensImporter(SensorContext context, Predicate<InputFile> inputFileFilter) {
    return new CPDTokensImporter(context, inputFileFilter);
  }

  public static RawProtobufImporter metricsImporter(SensorContext context, FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter,
    Predicate<InputFile> inputFileFilter) {
    return new MetricsImporter(context, fileLinesContextFactory, noSonarFilter, inputFileFilter);
  }

  public static RawProtobufImporter issuesImporter(SensorContext context, String repositoryKey, Predicate<InputFile> inputFileFilter) {
    return new IssuesImporter(context, repositoryKey, inputFileFilter);
  }

  public static EncodingImporter encodingImporter() {
    return new EncodingImporter();
  }
}
