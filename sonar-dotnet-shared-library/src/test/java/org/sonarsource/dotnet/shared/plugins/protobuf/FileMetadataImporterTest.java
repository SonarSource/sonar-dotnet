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

import com.google.protobuf.AbstractParser;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.net.URI;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Paths;
import java.util.Map;
import java.util.Set;
import org.junit.Ignore;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.FILEMETADATA_OUTPUT_PROTOBUF_NAME;

public class FileMetadataImporterTest {
  @Rule
  public LogTester logs = new LogTester();

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";

  private AbstractParser<SonarAnalyzer.FileMetadataInfo> parser = mock(AbstractParser.class);
  private FileMetadataImporter fileMetadataImporter = new FileMetadataImporter(parser);
  private SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);

  private File protobuf = new File(TEST_DATA_DIR, FILEMETADATA_OUTPUT_PROTOBUF_NAME);
  private File invalidProtobuf = new File(TEST_DATA_DIR, "invalid-encoding.pb");

  @Test
  public void getGeneratedFilePaths_returns_only_generated_uris() {
    SonarAnalyzer.FileMetadataInfo.Builder builder = SonarAnalyzer.FileMetadataInfo.newBuilder();

    SonarAnalyzer.FileMetadataInfo message1 = builder.setIsGenerated(true).setFilePath("c:\\file1").build();
    SonarAnalyzer.FileMetadataInfo message2 = builder.setIsGenerated(true).setFilePath("/usr/local/src/project/file2").build();
    SonarAnalyzer.FileMetadataInfo message3 = builder.setIsGenerated(false).setFilePath("c:\\file3").build();
    SonarAnalyzer.FileMetadataInfo messageSamePath = builder.setIsGenerated(false).setFilePath("c:\\file3").build();

    fileMetadataImporter.consume(message1);
    fileMetadataImporter.consume(message2);
    fileMetadataImporter.consume(message3);
    fileMetadataImporter.consume(messageSamePath);

    // Act
    Set<URI> uris = fileMetadataImporter.getGeneratedFileUris();

    // Assert
    assertThat(uris.size()).isEqualTo(2);
    assertThat(uris.contains(Paths.get("c:\\file1").toUri())).isTrue();
    assertThat(uris.contains(Paths.get("/usr/local/src/project/file2").toUri())).isTrue();
    assertThat(uris.contains(Paths.get("c:\\file3").toUri())).isFalse();
  }

  @Test
  public void getGeneratedFileUris_returns_empty_set_when_protobuf_is_empty() {
    // No consume calls means that the protobuf contained no messages

    // Act
    Set<URI> uris = fileMetadataImporter.getGeneratedFileUris();

    // Assert
    assertThat(uris.isEmpty()).isTrue();
  }

  @Ignore("this can be used to regenerate the files in case of a change in the protobuf definition")
  @Test
  public void regenerate_test_files() throws IOException {
    SonarAnalyzer.FileMetadataInfo.newBuilder()
      .setFilePath(TEST_FILE_PATH).setIsGenerated(false).setEncoding("UTF-7")
      .build().writeDelimitedTo(new FileOutputStream(invalidProtobuf));

    SonarAnalyzer.FileMetadataInfo.newBuilder()
      .setFilePath(TEST_FILE_PATH).setIsGenerated(false).setEncoding("UTF-8")
      .build().writeDelimitedTo(new FileOutputStream(protobuf));
  }

  @Test
  public void test_encoding_get_imported() throws FileNotFoundException {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .build();
    tester.fileSystem().add(inputFile);

    FileMetadataImporter metadataImporter = new FileMetadataImporter();
    metadataImporter.accept(protobuf.toPath());

    Map<URI, Charset> encodingPerUri = metadataImporter.getEncodingPerUri();
    assertThat(encodingPerUri).hasSize(1)
      .containsEntry(Paths.get(TEST_FILE_PATH).toUri(), StandardCharsets.UTF_8);
  }

  @Test
  public void test_encoding_warns_for_invalid_encoding() {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .build();
    tester.fileSystem().add(inputFile);

    FileMetadataImporter metadataImporter = new FileMetadataImporter();
    metadataImporter.accept(invalidProtobuf.toPath());

    assertThat(logs.logs(LoggerLevel.WARN)).containsOnly("Unrecognized encoding UTF-7 for file Program.cs");
  }
}
