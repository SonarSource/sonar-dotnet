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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.MetricsInfo;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class ProtobufDataImporterTest {
  @Rule
  public LogTester logTester = new LogTester();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private FileLinesContextFactory fileLinesContextFactory = mock(FileLinesContextFactory.class);
  private NoSonarFilter noSonarFilter = mock(NoSonarFilter.class);
  private FileLinesContext fileLinesContext = mock(FileLinesContext.class);;

  private SensorContextTester tester;
  private ProtobufDataImporter dataImporter = new ProtobufDataImporter(fileLinesContextFactory, noSonarFilter);
  private Path workDir;
  private InputFile inputFile;

  @Before
  public void prepare() throws Exception {
    workDir = temp.newFolder().toPath();
    Path csFile = Paths.get("src/test/resources/Program.cs").toAbsolutePath();

    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir);

    inputFile = new TestInputFileBuilder(tester.module().key(), "Program.cs")
      .setLanguage("cs")
      .initMetadata(new String(Files.readAllBytes(csFile), StandardCharsets.UTF_8))
      .build();
    tester.fileSystem().add(inputFile);

    when(fileLinesContextFactory.createFor(inputFile)).thenReturn(fileLinesContext);

    // create metrics.pb
    try (OutputStream os = Files.newOutputStream(workDir.resolve("metrics.pb"))) {
      MetricsInfo.newBuilder().setFilePath(inputFile.relativePath()).addCodeLine(12).addCodeLine(13).build().writeDelimitedTo(os);
    }
  }

  @Test
  public void should_import_existing_data() {
    dataImporter.importResults(tester, Collections.singletonList(workDir), String::toString);

    assertThat(tester.measures(inputFile.key())).isNotEmpty();
    assertThat(tester.measure(inputFile.key(), CoreMetrics.NCLOC).value()).isEqualTo(2);
  }

  @Test
  public void warn_about_files_not_found() {
    dataImporter.importResults(tester, Collections.singletonList(workDir), String::toString);

    String prefix = "Protobuf file not found: ";
    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly(
      prefix + workDir.resolve("token-type.pb"),
      prefix + workDir.resolve("symrefs.pb"),
      prefix + workDir.resolve("token-cpd.pb"));
  }
}
