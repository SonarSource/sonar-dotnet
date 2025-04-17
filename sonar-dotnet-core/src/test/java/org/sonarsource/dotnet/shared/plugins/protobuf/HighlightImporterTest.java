/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.highlighting.TypeOfText;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.mock;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.HIGHLIGHT_FILENAME;

public class HighlightImporterTest {

  @Rule
  public LogTester logs = new LogTester();

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  @Test
  public void test_syntax_highlights_get_imported() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);

    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    File protobuf = new File(TEST_DATA_DIR, HIGHLIGHT_FILENAME);
    assertThat(protobuf).withFailMessage("no such file: " + protobuf).isFile();

    HighlightImporter importer = new HighlightImporter(tester, String::toString);
    importer.accept(protobuf.toPath());
    importer.save();

    // using System;
    assertThat(tester.highlightingTypeAt(inputFile.key(), 1, 0).get(0)).isEqualTo(TypeOfText.KEYWORD);
    assertThat(tester.highlightingTypeAt(inputFile.key(), 1, 4).get(0)).isEqualTo(TypeOfText.KEYWORD);
    assertThat(tester.highlightingTypeAt(inputFile.key(), 1, 5)).isEmpty();

    // [SuppressMessage("Maintainability", "S2326:Unused type parameters should be removed")]
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 4)).isEmpty();
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 5).get(0)).isEqualTo(TypeOfText.KEYWORD_LIGHT);
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 19).get(0)).isEqualTo(TypeOfText.KEYWORD_LIGHT);
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 20)).isEmpty();
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 21).get(0)).isEqualTo(TypeOfText.STRING);
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 37).get(0)).isEqualTo(TypeOfText.STRING);
    assertThat(tester.highlightingTypeAt(inputFile.key(), 53, 38)).isEmpty();

    // if (op1 == 0)
    assertThat(tester.highlightingTypeAt(inputFile.key(), 27, 23).get(0)).isEqualTo(TypeOfText.CONSTANT);

    assertThat(tester.highlightingTypeAt(inputFile.key(), 9, 0).get(0)).isEqualTo(TypeOfText.COMMENT);
  }

  @Test
  public void test_syntax_highlights_overlap() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
    var inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);
    HighlightImporter importer = new HighlightImporter(tester, String::toString);
    var message = SonarAnalyzer.TokenTypeInfo.newBuilder()
      .setFilePath(inputFile.filename())
      .addTokenInfo(SonarAnalyzer.TokenTypeInfo.TokenInfo.newBuilder()
        .setTokenType(SonarAnalyzer.TokenType.KEYWORD)
        .setTextRange(SonarAnalyzer.TextRange.newBuilder()
          .setStartLine(1).setStartOffset(1)
          .setEndLine(1).setEndOffset(8)
        ).build())
      .addTokenInfo(SonarAnalyzer.TokenTypeInfo.TokenInfo.newBuilder()
        .setTokenType(SonarAnalyzer.TokenType.KEYWORD)
        .setTextRange(SonarAnalyzer.TextRange.newBuilder()
          .setStartLine(1).setStartOffset(5)
          .setEndLine(1).setEndOffset(10)
        ).build())
      .addTokenInfo(SonarAnalyzer.TokenTypeInfo.TokenInfo.newBuilder()
        .setTokenType(SonarAnalyzer.TokenType.KEYWORD)
        .setTextRange(SonarAnalyzer.TextRange.newBuilder()
          .setStartLine(2).setStartOffset(2)
          .setEndLine(2).setEndOffset(11)
        ).build()
      ).build();
    importer.consume(message);
    assertThatThrownBy(importer::save)
      .isInstanceOf(RuntimeException.class)
      .hasMessage("The highlighting in the file Program.cs failed with error java.lang.IllegalStateException: Cannot register highlighting rule for characters at " +
        "Range[from [line=1, lineOffset=5] to [line=1, lineOffset=10]] as it overlaps at least one existing rule. The highlight ranges found are " +
        "Range[from [line=1, lineOffset=1] to [line=1, lineOffset=8]] " +
        "Range[from [line=1, lineOffset=5] to [line=1, lineOffset=10]] " +
        "Range[from [line=2, lineOffset=2] to [line=2, lineOffset=11]] ");
    assertThat(logs.logs(Level.ERROR)).isEmpty();
  }

  @Test
  public void test_syntax_highlights_empty() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
    var inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);
    HighlightImporter importer = new HighlightImporter(tester, String::toString);
    var message = SonarAnalyzer.TokenTypeInfo.newBuilder()
      .setFilePath(inputFile.filename())
      .addTokenInfo(SonarAnalyzer.TokenTypeInfo.TokenInfo.newBuilder()
        .setTokenType(SonarAnalyzer.TokenType.UNKNOWN_TOKENTYPE)
        .setTextRange(SonarAnalyzer.TextRange.newBuilder()
          .setStartLine(1).setStartOffset(1)
          .setEndLine(1).setEndOffset(5)
        ).build())
      .build();
    importer.consume(message);
    importer.save();
    assertThat(logs.logs()).isEmpty();
  }

  @Test
  public void test_syntax_highlights_outOfRange() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
    var inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);
    HighlightImporter importer = new HighlightImporter(tester, String::toString);
    var message = SonarAnalyzer.TokenTypeInfo.newBuilder()
      .setFilePath(inputFile.filename())
      .addTokenInfo(SonarAnalyzer.TokenTypeInfo.TokenInfo.newBuilder()
        .setTokenType(SonarAnalyzer.TokenType.KEYWORD)
        .setTextRange(SonarAnalyzer.TextRange.newBuilder()
          .setStartLine(1_000_000).setStartOffset(1_000_000)
          .setEndLine(1_000_000).setEndOffset(1_000_000)
        ).build())
      .build();
    importer.consume(message);
    assertThatThrownBy(importer::save)
      .isInstanceOf(IllegalArgumentException.class)
      .hasMessage("1000000 is not a valid line for pointer. File Program.cs has 63 line(s)");
  }

  @Test
  public void test_syntax_highlights_invalidRange() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
    var inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);
    HighlightImporter importer = new HighlightImporter(tester, String::toString);
    var message = SonarAnalyzer.TokenTypeInfo.newBuilder()
      .setFilePath(inputFile.filename())
      .addTokenInfo(SonarAnalyzer.TokenTypeInfo.TokenInfo.newBuilder()
        .setTokenType(SonarAnalyzer.TokenType.KEYWORD)
        .setTextRange(SonarAnalyzer.TextRange.newBuilder()
          .setStartLine(2).setStartOffset(5)
          .setEndLine(1).setEndOffset(6)
        ).build())
      .build();
    importer.consume(message);
    logs.setLevel(Level.DEBUG);
    importer.save();
    assertThat(logs.logs(Level.DEBUG)).containsOnly("""
      The reported token was out of the range. File Program.cs, Range start_line: 2
      end_line: 1
      start_offset: 5
      end_offset: 6
      """);
  }
}
