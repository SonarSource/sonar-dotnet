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

import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.notifications.AnalysisWarnings;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.Collections;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.METRICS_FILENAME;

public class RoslynMetricsImporterTest {
  private static final File TEST_DATA_DIR = new File("src/test/resources/RazorProtobufImporter");
  private static final String TEST_FILE_PATH = "Cases.razor";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);
  private static final File PROTOBUF_FILE = new File(TEST_DATA_DIR, METRICS_FILENAME);

  @Before
  public void before() {
    assertThat(TEST_FILE).withFailMessage("no such file: " + TEST_FILE).isFile();
    assertThat(PROTOBUF_FILE).withFailMessage("no such file: " + PROTOBUF_FILE).isFile();
  }

  @Test
  public void roslyn_metrics_are_imported() throws FileNotFoundException {
    var inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(TEST_FILE)))
      .build();

    var sensorContext = SensorContextTester.create(TEST_FILE);
    sensorContext.fileSystem().add(inputFile);

    var noSonarFilter = mock(NoSonarFilter.class);
    var fileLinesContext = mock(FileLinesContext.class);
    var fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(fileLinesContext);

    new MetricsImporter(sensorContext, fileLinesContextFactory, noSonarFilter, String::toString).accept(PROTOBUF_FILE.toPath());

    var measures = sensorContext.measures(inputFile.key());
    assertThat(measures).hasSize(7);

    assertThat(measures).extracting("metric", "value")
      .containsOnly(
        Tuple.tuple(CoreMetrics.CLASSES, 1),
        Tuple.tuple(CoreMetrics.STATEMENTS, 15),
        Tuple.tuple(CoreMetrics.FUNCTIONS, 4),
        Tuple.tuple(CoreMetrics.COMPLEXITY, 4),
        Tuple.tuple(CoreMetrics.COMMENT_LINES, 0),
        Tuple.tuple(CoreMetrics.COGNITIVE_COMPLEXITY, 0),
        Tuple.tuple(CoreMetrics.NCLOC, 12));

    verify(noSonarFilter).noSonarInFile(inputFile, Collections.emptySet());

    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 3, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 5, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 9, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 19, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 20, 1);

    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 3, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 5, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 7, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 9, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 12, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 14, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 15, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 17, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 18, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 19, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 20, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 21, 1);
  }
}
