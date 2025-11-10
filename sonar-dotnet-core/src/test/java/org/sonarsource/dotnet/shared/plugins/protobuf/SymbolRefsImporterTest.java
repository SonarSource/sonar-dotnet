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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.notifications.AnalysisWarnings;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.SYMBOLREFS_FILENAME;

public class SymbolRefsImporterTest {

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  private SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
  private File protobuf = new File(TEST_DATA_DIR, SYMBOLREFS_FILENAME);
  SymbolRefsImporter underTest = new SymbolRefsImporter(tester, String::toString);

  @Before
  public void setUp() {
    assertThat(protobuf).withFailMessage("no such file: " + protobuf).isFile();
  }

  @Test
  public void test_symbolrefs_get_imported() throws FileNotFoundException {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    underTest.accept(protobuf.toPath());
    underTest.save();

    // a symbol is defined at this location, and referenced at 3 other locations
    assertThat(tester.referencesForSymbolAt(inputFile.key(), 25, 34)).hasSize(3);

    // ... other similar examples ...
    assertThat(tester.referencesForSymbolAt(inputFile.key(), 25, 43)).hasSize(3);
    assertThat(tester.referencesForSymbolAt(inputFile.key(), 56, 30)).hasSize(1);
    assertThat(tester.referencesForSymbolAt(inputFile.key(), 56, 37)).hasSize(1);
  }
}
