/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

import com.sonarsource.scanner.engine.sensor.test.fixtures.SensorContextTester;
import java.io.File;
import java.util.function.UnaryOperator;
import org.junit.Assume;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonar.scanner.plugin.api.impl.config.MapSettings;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;

public class ProtobufImporterTest {

  @Rule
  public LogTester logs = new LogTester();

  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";

  @Test
  public void exclusion_viaSonarExclusions_logsDebug() {
    assertExcludedFileLogsDebug("sonar.exclusions", TEST_FILE_PATH);
  }

  @Test
  public void exclusion_viaSonarTestExclusions_logsDebug() {
    assertExcludedFileLogsDebug("sonar.test.exclusions", TEST_FILE_PATH);
  }

  @Test
  public void exclusion_viaSonarGlobalExclusions_logsDebug() {
    assertExcludedFileLogsDebug("sonar.global.exclusions", TEST_FILE_PATH);
  }

  @Test
  public void exclusion_viaSonarGlobalTestExclusions_logsDebug() {
    assertExcludedFileLogsDebug("sonar.global.test.exclusions", TEST_FILE_PATH);
  }

  @Test
  public void exclusion_viaSonarTestsExclusions_logsDebug() {
    assertExcludedFileLogsDebug("sonar.tests.exclusions", TEST_FILE_PATH);
  }

  @Test
  public void exclusion_matchesAbsolutePath() {
    assertExcludedFileLogsDebug("sonar.exclusions", new File(TEST_DATA_DIR, TEST_FILE_PATH).getAbsolutePath());
  }

  @Test
  public void missingFile_NotExcluded_Warns() {
    TestProtobufImporter importer = new TestProtobufImporter(SensorContextTester.create(TEST_DATA_DIR), String::toString);
    var message = SonarAnalyzer.TokenTypeInfo.newBuilder().setFilePath("Missing.cs").build();

    importer.consume(message);
    assertThat(logs.logs(Level.WARN)).containsExactly("File 'Missing.cs' referenced by the protobuf 'TokenTypeInfo' does not exist in the analysis context");
  }

  @Test
  public void path_on_different_drive_is_treated_as_not_excluded() {
    Assume.assumeTrue(System.getProperty("os.name").toLowerCase().startsWith("win"));
    String crossDrivePath = "W:\\does\\not\\exist\\Program.cs";
    TestProtobufImporter importer = new TestProtobufImporter(
      SensorContextTester.create(TEST_DATA_DIR).setSettings(new MapSettings().setProperty("sonar.exclusions", "**/Program.cs")), String::toString);
    logs.setLevel(Level.DEBUG);

    importer.consume(SonarAnalyzer.TokenTypeInfo.newBuilder().setFilePath(crossDrivePath).build());
    assertThat(logs.logs(Level.WARN)).containsExactly("File '" + crossDrivePath + "' referenced by the protobuf 'TokenTypeInfo' does not exist in the analysis context");
    assertThat(logs.logs(Level.DEBUG)).noneMatch(line -> line.contains("is excluded from the analysis"));
  }

  private void assertExcludedFileLogsDebug(String setting, String filePath) {
    // No input file is added to the file system, so SensorContextUtils.toInputFile returns null.
    TestProtobufImporter importer = new TestProtobufImporter(SensorContextTester.create(TEST_DATA_DIR)
      .setSettings(new MapSettings().setProperty(setting, "**/Program.cs")), String::toString);
    logs.setLevel(Level.DEBUG);

    importer.consume(SonarAnalyzer.TokenTypeInfo.newBuilder().setFilePath(filePath).build());
    assertThat(logs.logs(Level.WARN)).isEmpty();
    assertThat(logs.logs(Level.DEBUG)).containsExactly("File '" + filePath + "' referenced by the protobuf 'TokenTypeInfo' is excluded from the analysis");
  }

  private static final class TestProtobufImporter extends ProtobufImporter<SonarAnalyzer.TokenTypeInfo> {
    TestProtobufImporter(SensorContext context, UnaryOperator<String> toRealPath) {
      super(SonarAnalyzer.TokenTypeInfo.parser(), context, SonarAnalyzer.TokenTypeInfo::getFilePath, toRealPath);
    }

    @Override
    void consumeFor(InputFile inputFile, SonarAnalyzer.TokenTypeInfo message) {
      //  these tests only exercise the base ProtobufImporter exclusion and handling of missing files
    }
  }
}
