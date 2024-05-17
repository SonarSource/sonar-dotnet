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
import java.util.Collections;
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
  private static final File PROTOBUF_FILE = new File(TEST_DATA_DIR, METRICS_FILENAME);

  @Before
  @Override
  public void setUp() throws FileNotFoundException {
    super.setUp();
    assertThat(PROTOBUF_FILE).withFailMessage("no such file: " + PROTOBUF_FILE).isFile();
  }

  @Test
  public void roslyn_metrics_are_imported() {
    var inputFile = CasesInputFile;
    var noSonarFilter = mock(NoSonarFilter.class);
    var fileLinesContext = mock(FileLinesContext.class);
    var fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(any(InputFile.class))).thenReturn(fileLinesContext);

    new MetricsImporter(sensorContext, fileLinesContextFactory, noSonarFilter, RazorImporterTestBase::fileName).accept(PROTOBUF_FILE.toPath());

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

    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 8, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 23, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.EXECUTABLE_LINES_DATA_KEY, 24, 1);

    verify(fileLinesContext, times(4)).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 1, 1);
    verify(fileLinesContext, times(2)).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 3, 1);
    verify(fileLinesContext, times(2)).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 5, 1);
    verify(fileLinesContext, times(2)).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 5, 1);

    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 8, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 9, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 13, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 16, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 18, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 19, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 21, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 22, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 23, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 24, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 25, 1);

    assertThat(logTester.logs(Level.DEBUG)).containsExactly("The code line number was out of the range. File Page.razor, Line 4");
  }
}
