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

import java.io.File;
import java.io.FileNotFoundException;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Map;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ENCODING_OUTPUT_PROTOBUF_NAME;

public class EncodingImporterTest {

  @Rule
  public LogTester logs = new LogTester();

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";

  private SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
  private File protobuf = new File(TEST_DATA_DIR, ENCODING_OUTPUT_PROTOBUF_NAME);
  private File invalidProtobuf = new File(TEST_DATA_DIR, "invalid-encoding.pb");
  private EncodingImporter importer = new EncodingImporter();

  @Before
  public void setUp() {
    assertThat(protobuf.isFile()).withFailMessage("no such file: " + protobuf).isTrue();
  }

  @Test
  public void test_encoding_get_imported() throws FileNotFoundException {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .build();
    tester.fileSystem().add(inputFile);

    importer.accept(protobuf.toPath());

    Map<Path, Charset> encodingPerPath = importer.getEncodingPerPath();
    assertThat(encodingPerPath.size()).isEqualTo(1);
    assertThat(encodingPerPath.get(Paths.get(TEST_FILE_PATH))).isEqualTo(StandardCharsets.UTF_8);
  }

  @Test
  public void test_encoding_warns_for_invalid_encoding() {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .build();
    tester.fileSystem().add(inputFile);

    importer.accept(invalidProtobuf.toPath());

    assertThat(logs.logs(LoggerLevel.WARN)).containsOnly("Unrecognized encoding utf-7 for file Program.cs");
  }
}
