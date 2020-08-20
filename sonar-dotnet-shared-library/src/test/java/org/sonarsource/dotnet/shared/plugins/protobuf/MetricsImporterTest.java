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

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.Collection;
import java.util.Collections;
import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.batch.sensor.measure.Measure;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.METRICS_OUTPUT_PROTOBUF_NAME;

public class MetricsImporterTest {

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  private SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);
  private File protobuf = new File(TEST_DATA_DIR, METRICS_OUTPUT_PROTOBUF_NAME);

  @Before
  public void before() {
    assertThat(protobuf).withFailMessage("no such file: " + protobuf).isFile();
  }

  @Test
  public void test_metrics_get_imported() throws FileNotFoundException {
    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata().readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    FileLinesContext fileLinesContext = mock(FileLinesContext.class);
    FileLinesContextFactory fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(fileLinesContext);

    NoSonarFilter noSonarFilter = mock(NoSonarFilter.class);

    new MetricsImporter(tester, fileLinesContextFactory, noSonarFilter, String::toString).accept(protobuf.toPath());

    Collection<Measure> measures = tester.measures(inputFile.key());
    assertThat(measures).hasSize(7);

    // TODO change test data so that all metrics have different expected values

    assertThat(measures).extracting("metric", "value")
      .containsOnly(
        Tuple.tuple(CoreMetrics.CLASSES, 4),
        Tuple.tuple(CoreMetrics.STATEMENTS, 6),
        Tuple.tuple(CoreMetrics.FUNCTIONS, 3),
        Tuple.tuple(CoreMetrics.COMPLEXITY, 7),
        Tuple.tuple(CoreMetrics.COMMENT_LINES, 12),
        Tuple.tuple(CoreMetrics.COGNITIVE_COMPLEXITY, 18),
        Tuple.tuple(CoreMetrics.NCLOC, 41));

    verify(noSonarFilter).noSonarInFile(inputFile, Collections.singleton(49));

    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 27, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 29, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 32, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 34, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 37, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 58, 1);

    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 1, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 2, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 3, 1);
  }

}
