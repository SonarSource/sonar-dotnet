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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toTextRange;

import java.util.ArrayList;
import java.util.function.UnaryOperator;
import javax.annotation.CheckForNull;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.TextRange;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.highlighting.NewHighlighting;
import org.sonar.api.batch.sensor.highlighting.TypeOfText;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.TokenTypeInfo;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;

/**
 * This class is responsible for reading/importing the highlight info that was processed by the C#/VB.NET analyzer.
 */
public class HighlightImporter extends ProtobufImporter<SonarAnalyzer.TokenTypeInfo> {

  private static final Logger LOG = LoggerFactory.getLogger(HighlightImporter.class);
  private final SensorContext context;
  private final Map<InputFile, HashSet<TokenTypeInfo.TokenInfo>> fileHighlights = new HashMap<>();

  public HighlightImporter(SensorContext context, UnaryOperator<String> toRealPath) {
    super(SonarAnalyzer.TokenTypeInfo.parser(), context, SonarAnalyzer.TokenTypeInfo::getFilePath, toRealPath);
    this.context = context;
  }

  @Override
  void consumeFor(InputFile inputFile, TokenTypeInfo message) {
    for (SonarAnalyzer.TokenTypeInfo.TokenInfo tokenInfo : message.getTokenInfoList()) {
      fileHighlights
        .computeIfAbsent(inputFile, f -> new HashSet<>())
        .add(tokenInfo);
    }
  }

  @Override
  public void save() {
    for (Map.Entry<InputFile, HashSet<SonarAnalyzer.TokenTypeInfo.TokenInfo>> entry : fileHighlights.entrySet()) {
      NewHighlighting highlighting = context.newHighlighting().onFile(entry.getKey());

      var ranges = new ArrayList<TextRange>();
      for (SonarAnalyzer.TokenTypeInfo.TokenInfo message : entry.getValue()) {
        TypeOfText typeOfText = toType(message.getTokenType());
        if (typeOfText != null) {
          var textRange = toTextRange(entry.getKey(), message.getTextRange());
          if (textRange.isPresent()) {
            ranges.add(textRange.get());
            highlighting.highlight(textRange.get(), typeOfText);
          } else if (LOG.isDebugEnabled()) {
            LOG.debug("The reported token was out of the range. File {}, Range {}", entry.getKey().filename(), message.getTextRange());
          }
        }
      }
      if (!ranges.isEmpty()) {
        doSave(entry.getKey().filename(), highlighting, ranges);
      }
    }
  }

  private static void doSave(String filename, NewHighlighting highlighting, ArrayList<TextRange> ranges) {
    try {
      highlighting.save();
    } catch (Exception ex) {
      var rangesText = ranges.stream()
        .sorted((r1, r2) ->
          r1.start().line() == r2.start().line() && r1.start().lineOffset() == r2.start().lineOffset()
            ? 0
            : r1.start().line() < r2.start().line() || (r1.start().line() == r2.start().line() && r1.start().lineOffset() < r2.start().lineOffset())
            ? -1 : 1)
        .collect(StringBuilder::new, (sb, r) -> sb.append(r).append(" "), StringBuilder::append);
      var message = String.format("The highlighting in the file %s failed with error %s. The highlight ranges found are %s", filename, ex, rangesText);
      throw new IllegalStateException(message, ex);
    }
  }

  @Override
  boolean isProcessed(InputFile inputFile) {
    // we aggregate all highlighting information, no need to process only the first protobuf file
    return false;
  }

  @CheckForNull
  private static TypeOfText toType(SonarAnalyzer.TokenType tokenType) {
    // Note:
    // TypeOfText.ANNOTATION -> like a type in C#, so received as DECLARATION_NAME
    // TypeOfText.STRUCTURED_COMMENT -> not colored differently in C#, so received as COMMENT
    // TypeOfText.PREPROCESS_DIRECTIVE -> received as KEYWORD

    switch (tokenType) {
      case NUMERIC_LITERAL:
        return TypeOfText.CONSTANT;

      case COMMENT:
        return TypeOfText.COMMENT;

      case KEYWORD:
        return TypeOfText.KEYWORD;

      case TYPE_NAME:
        return TypeOfText.KEYWORD_LIGHT;

      case STRING_LITERAL:
        return TypeOfText.STRING;

      case UNRECOGNIZED,
           // generated by protobuf
           UNKNOWN_TOKENTYPE:

      default:
        // do not color
        return null;
    }
  }
}
