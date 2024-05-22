/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.nio.file.Paths;
import org.junit.Before;
import org.junit.Rule;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class RazorImporterTestBase {
  protected final static String TEST_DATA_DIR = "src/test/resources/RazorProtobufImporter";
  protected final static String WEB_PROJECT_PATH = Paths.get(TEST_DATA_DIR, "WebProject").toString();
  protected final static String ROSLYN_4_9_DIR = Paths.get(TEST_DATA_DIR, "Roslyn 4.9").toString();
  protected final static String ROSLYN_4_10_DIR = Paths.get(TEST_DATA_DIR, "Roslyn 4.10").toString();
  protected final SensorContextTester sensorContext = SensorContextTester.create(new File(TEST_DATA_DIR));

  @Rule
  public LogTester logTester = new LogTester();

  protected static String fileName(String filePath) {
    return Paths.get(filePath).getFileName().toString();
  }

  @Before
  public void setUp() {
    logTester.setLevel(Level.TRACE);
  }

  protected DefaultInputFile addTestFileToContext(String testFilePath) throws FileNotFoundException {
    var testFile = new File(WEB_PROJECT_PATH, testFilePath);
    assertThat(testFile).withFailMessage("no such file: " + testFilePath).isFile();
    var inputFile = new TestInputFileBuilder("dummyKey", testFilePath)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(testFile)))
      .build();
    sensorContext.fileSystem().add(inputFile);
    return inputFile;
  }
}
