/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2023 SonarSource SA
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

import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.SYMBOLREFS_FILENAME;

public class RoslynSymbolRefsImporterTest {

  private static final File TEST_DATA_DIR = new File("src/test/resources/RazorProtobufImporter");
  private static final String TEST_FILE_PATH = "Cases.razor";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  private final SensorContextTester sensorContext = SensorContextTester.create(TEST_DATA_DIR);
  private final File protobuf = new File(TEST_DATA_DIR, SYMBOLREFS_FILENAME);

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void setUp() {
    logTester.setLevel(Level.TRACE);
    assertThat(protobuf).withFailMessage("no such file: " + protobuf).isFile();
  }

  @Test
  public void test_symbol_refs_get_imported() throws FileNotFoundException {
    var inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();
    sensorContext.fileSystem().add(inputFile);

    var sut = new SymbolRefsImporter(sensorContext, String::toString);
    sut.accept(protobuf.toPath());
    sut.save();

    // a symbol is defined at this location, and referenced at 3 other locations
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 8, 30)).hasSize(1);

    // ... other similar examples ...
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 16, 16)).hasSize(4);
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 21, 17)).hasSize(1);
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 19, 15)).hasSize(3);

    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("The reported token was out of the range. File Cases.razor, Range start_line: 8");
  }
}
