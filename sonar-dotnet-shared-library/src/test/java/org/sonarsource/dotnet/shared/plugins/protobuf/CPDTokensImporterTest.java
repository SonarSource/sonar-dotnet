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
import java.io.FileReader;
import java.util.List;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.cpd.internal.TokensLine;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.CPDTOKENS_OUTPUT_PROTOBUF_NAME;

public class CPDTokensImporterTest {

  @Rule
  public LogTester logs = new LogTester();

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  private SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
  private CPDTokensImporter underTest = new CPDTokensImporter(tester, String::toString);
  private File protobuf = new File(TEST_DATA_DIR, CPDTOKENS_OUTPUT_PROTOBUF_NAME);

  @Before
  public void before() {
    assertThat(protobuf.isFile()).withFailMessage("no such file: " + protobuf).isTrue();
  }

  @Test
  public void test_copy_paste_tokens_get_imported() throws FileNotFoundException {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata().readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    underTest.accept(protobuf.toPath());

    List<TokensLine> lines = tester.cpdTokens(inputFile.key());
    checkExpectedData(lines);
  }

  @Test
  public void ignore_repeated_files() throws FileNotFoundException {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata().readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    underTest.accept(protobuf.toPath());
    underTest.accept(protobuf.toPath());

    List<TokensLine> lines = tester.cpdTokens(inputFile.key());
    checkExpectedData(lines);

    assertThat(logs.logs(LoggerLevel.DEBUG)).containsOnly("File 'Program.cs' was already processed. Skip it");
  }

  private void checkExpectedData(List<TokensLine> lines) {
    assertThat(lines).hasSize(35);

    assertThat(lines.get(0).getValue()).isEqualTo("namespaceConsoleApplication1");
    assertThat(lines.get(0).getStartLine()).isEqualTo(15);

    assertThat(lines.get(2).getValue()).isEqualTo("publicclassProgram");
    assertThat(lines.get(2).getStartLine()).isEqualTo(23);

    assertThat(lines.get(4).getValue()).isEqualTo("publicstaticintAdd(intop1,intop2)");
    assertThat(lines.get(4).getStartLine()).isEqualTo(25);

    assertThat(lines.get(5).getValue()).isEqualTo("{");
    assertThat(lines.get(5).getStartLine()).isEqualTo(26);

    assertThat(lines.get(6).getValue()).isEqualTo("if(op1==$num)");
    assertThat(lines.get(6).getStartLine()).isEqualTo(27);
  }

}
