/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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

import javax.annotation.CheckForNull;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.TextRange;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import java.util.Optional;

import static org.sonar.api.batch.fs.InputFile.Type;

public final class SensorContextUtils {
  private SensorContextUtils() {
    // utility class, forbidden constructor
  }

  @CheckForNull
  public static InputFile toInputFile(FileSystem fs, String file) {
    return fs.inputFile(fs.predicates().hasPath(file));
  }

  public static boolean hasFilesOfType(FileSystem fs, Type fileType, String languageKey) {
    FilePredicates p = fs.predicates();
    return fs.inputFiles(p.and(p.hasType(fileType), p.hasLanguage(languageKey))).iterator().hasNext();
  }

  public static boolean hasFilesOfLanguage(FileSystem fs, String languageKey) {
    FilePredicates p = fs.predicates();
    return fs.inputFiles(p.hasLanguage(languageKey)).iterator().hasNext();
  }

  public static boolean hasAnyMainFiles(FileSystem fs) {
    return fs.inputFiles(fs.predicates().hasType(Type.MAIN)).iterator().hasNext();
  }

  public static Optional<TextRange> toTextRange(InputFile inputFile, SonarAnalyzer.TextRange pbTextRange) {
    // We accept data out of range due to the mapping issues on Roslyn side.
    // The strategy is to throw if the start offset is outside the line; otherwise, if only the end line is out of the range,
    // trim to the end of the line.
    // The range is discarded if trimming of the end has made it empty.
    // The wrong locations are caused by the following issues:
    // https://github.com/dotnet/roslyn/issues/69248
    // https://github.com/dotnet/razor/issues/9051
    // https://github.com/dotnet/razor/issues/9050
    int startLine = pbTextRange.getStartLine();
    int startLineOffset = pbTextRange.getStartOffset();
    int startLineLength = inputFile.selectLine(startLine).end().lineOffset();
    if (startLineOffset > startLineLength) {
      startLine++;
      startLineOffset = 0;
    }

    int endLine = pbTextRange.getEndLine();
    int endLineLength = inputFile.selectLine(endLine).end().lineOffset();
    int endLineOffset = pbTextRange.getEndOffset();
    if (endLineOffset > endLineLength) {
      endLineOffset = endLineLength;
    }

    return startLine < endLine || (startLine == endLine && startLineOffset < endLineOffset)
      ? Optional.of(inputFile.newRange(startLine, startLineOffset, endLine, endLineOffset))
      : Optional.empty();
  }
}
