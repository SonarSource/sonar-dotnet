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
import java.util.Arrays;
import java.util.Collections;
import java.util.function.Function;
import java.util.stream.Collectors;

import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.METRICS_FILENAME;

public class RazorMetricsImporterTest extends RazorImporterTestBase {
  private static final File PROTOBUF_4_9_FILE = new File(ROSLYN_4_9_DIR, METRICS_FILENAME);
  private static final File PROTOBUF_4_10_FILE = new File(ROSLYN_4_10_DIR, METRICS_FILENAME);

  @Before
  @Override
  public void setUp() {
    super.setUp();
    assertThat(PROTOBUF_4_9_FILE).withFailMessage("no such file: " + PROTOBUF_4_9_FILE).isFile();
    assertThat(PROTOBUF_4_10_FILE).withFailMessage("no such file: " + PROTOBUF_4_10_FILE).isFile();
  }

  @Test
  public void roslyn_metrics_are_imported_before_4_10() throws FileNotFoundException {
    var inputFile = addTestFileToContext("Cases.razor");
    var noSonarFilter = mock(NoSonarFilter.class);
    var fileLinesContext = mock(FileLinesContext.class);
    var fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(fileLinesContext);

    new MetricsImporter(sensorContext, fileLinesContextFactory, noSonarFilter, RazorImporterTestBase::fileName).accept(PROTOBUF_4_9_FILE.toPath());

    var measures = sensorContext.measures(inputFile.key());
    assertThat(measures).hasSize(7);

    assertThat(measures).extracting("metric", "value")
      .containsOnly(
        Tuple.tuple(CoreMetrics.COMPLEXITY, 5),
        Tuple.tuple(CoreMetrics.FUNCTIONS, 3),
        Tuple.tuple(CoreMetrics.COMMENT_LINES, 0),
        Tuple.tuple(CoreMetrics.COGNITIVE_COMPLEXITY, 1),
        Tuple.tuple(CoreMetrics.CLASSES, 0),
        Tuple.tuple(CoreMetrics.NCLOC, 13),
        Tuple.tuple(CoreMetrics.STATEMENTS, 6));

    verify(noSonarFilter).noSonarInFile(inputFile, Collections.emptySet());

    verifyMetrics(fileLinesContext, CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 8, 23, 24);
    verifyMetrics(fileLinesContext, CoreMetrics.NCLOC_DATA_KEY, 3, 5, 8, 9, 13, 16, 18, 19, 21, 22, 23, 24, 25);

    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void roslyn_metrics_are_imported_starting_with_4_10() throws FileNotFoundException {
    var inputFile = addTestFileToContext("Cases.razor");
    var noSonarFilter = mock(NoSonarFilter.class);
    var fileLinesContext = mock(FileLinesContext.class);
    var fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(fileLinesContext);

    new MetricsImporter(sensorContext, fileLinesContextFactory, noSonarFilter, RazorImporterTestBase::fileName).accept(PROTOBUF_4_10_FILE.toPath());

    var measures = sensorContext.measures(inputFile.key());
    assertThat(measures).hasSize(7);

    assertThat(measures).extracting("metric", "value")
      .containsOnly(
        Tuple.tuple(CoreMetrics.COMPLEXITY, 5),
        Tuple.tuple(CoreMetrics.FUNCTIONS, 3),
        Tuple.tuple(CoreMetrics.COMMENT_LINES, 0),
        Tuple.tuple(CoreMetrics.COGNITIVE_COMPLEXITY, 1),
        Tuple.tuple(CoreMetrics.CLASSES, 0),
        Tuple.tuple(CoreMetrics.NCLOC, 14),
        Tuple.tuple(CoreMetrics.STATEMENTS, 3));

    verify(noSonarFilter).noSonarInFile(inputFile, Collections.emptySet());

    verifyMetrics(fileLinesContext, CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 8, 23, 24);
    verifyMetrics(fileLinesContext, CoreMetrics.NCLOC_DATA_KEY, 1, 3, 5, 8, 9, 13, 16, 18, 19, 21, 22, 23, 24, 25);

    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void roslyn_metrics_out_of_range_with_4_10_debug_enabled() throws FileNotFoundException {
    addTestFileToContext("_Imports.razor");
    var fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(mock(FileLinesContext.class));

    new MetricsImporter(sensorContext, fileLinesContextFactory, mock(NoSonarFilter.class), RazorImporterTestBase::fileName).accept(PROTOBUF_4_10_FILE.toPath());

    assertThat(logTester.logs(Level.DEBUG)).containsExactly("The code line number was out of the range. File _Imports.razor, Line 4");
  }

  @Test
  public void roslyn_metrics_out_of_range_with_4_10_debug_disabled() throws FileNotFoundException {
    logTester.setLevel(Level.INFO);
    addTestFileToContext("_Imports.razor");
    var fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(mock(FileLinesContext.class));

    new MetricsImporter(sensorContext, fileLinesContextFactory, mock(NoSonarFilter.class), RazorImporterTestBase::fileName).accept(PROTOBUF_4_10_FILE.toPath());

    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  private void verifyMetrics(FileLinesContext context, String key, int... values) {
    var groups = Arrays.stream(values)
      .boxed()
      .collect(Collectors.groupingBy(Function.identity(), Collectors.counting()));

    for (int groupKey : groups.keySet()){
      verify(context, times(groups.get(groupKey).intValue())).setIntValue(key, groupKey, 1);
    }
  }
}
