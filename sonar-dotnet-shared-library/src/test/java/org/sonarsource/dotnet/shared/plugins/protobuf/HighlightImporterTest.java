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

import org.junit.Test;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.highlighting.TypeOfText;
import org.sonar.api.batch.sensor.internal.SensorContextTester;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;

public class HighlightImporterTest {

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  @Test
  public void test_syntax_highlights_get_imported() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);

    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata().readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    File protobuf = new File(TEST_DATA_DIR, HIGHLIGHT_OUTPUT_PROTOBUF_NAME);
    assertThat(protobuf.isFile()).withFailMessage("no such file: " + protobuf).isTrue();

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

}
