/*
 * Sonar .NET Plugin :: Tests
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.tests;

import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableSet;
import org.junit.Test;
import org.mockito.ArgumentCaptor;
import org.mockito.Mockito;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Language;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;

import java.util.List;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CoverageReportImportSensorTest {

  @Test
  public void should_execute_on_project() {
    Project project = mock(Project.class);

    CoverageParserFactory coverageFactoryWithReports = mock(CoverageParserFactory.class);

    when(coverageFactoryWithReports.hasCoverageProperty()).thenReturn(true);
    assertThat(new CoverageReportImportSensor(coverageFactoryWithReports).shouldExecuteOnProject(project)).isTrue();

    when(coverageFactoryWithReports.hasCoverageProperty()).thenReturn(false);
    assertThat(new CoverageReportImportSensor(coverageFactoryWithReports).shouldExecuteOnProject(project)).isFalse();
  }

  @Test
  public void analyze() {
    Coverage coverage = mock(Coverage.class);
    when(coverage.files()).thenReturn(ImmutableSet.of("Foo.cs", "Bar.cs", "Baz.java"));
    when(coverage.hits("Foo.cs")).thenReturn(ImmutableMap.<Integer, Integer>builder()
      .put(24, 1)
      .put(42, 0)
      .build());
    when(coverage.hits("Bar.cs")).thenReturn(ImmutableMap.<Integer, Integer>builder()
      .put(42, 1)
      .build());
    when(coverage.hits("Baz.java")).thenReturn(ImmutableMap.<Integer, Integer>builder()
      .put(42, 1)
      .build());

    CoverageParser coverageProvider = mock(CoverageParser.class);
    when(coverageProvider.parse()).thenReturn(coverage);

    CoverageParserFactory coverageProviderFactory = mock(CoverageParserFactory.class);
    when(coverageProviderFactory.coverageProvider()).thenReturn(coverageProvider);

    SensorContext context = mock(SensorContext.class);

    FileProvider fileProvider = mock(FileProvider.class);

    org.sonar.api.resources.File csSonarFile = mockSonarFile("cs");
    org.sonar.api.resources.File javaSonarFile = mockSonarFile("java");

    when(fileProvider.fromPath("Foo.cs")).thenReturn(csSonarFile);
    when(fileProvider.fromPath("Bar.cs")).thenReturn(null);
    when(fileProvider.fromPath("Baz.java")).thenReturn(javaSonarFile);

    new CoverageReportImportSensor(coverageProviderFactory).analyze(context, fileProvider);

    verify(context, Mockito.times(3)).saveMeasure(Mockito.any(Resource.class), Mockito.any(Measure.class));

    ArgumentCaptor<Measure> captor = ArgumentCaptor.forClass(Measure.class);
    verify(context, Mockito.times(3)).saveMeasure(Mockito.eq(csSonarFile), captor.capture());

    List<Measure> values = captor.getAllValues();
    checkMeasure(values.get(0), CoreMetrics.LINES_TO_COVER, 2.0);
    checkMeasure(values.get(1), CoreMetrics.UNCOVERED_LINES, 1.0);
  }

  private static void checkMeasure(Measure measure, Metric metric, Double value) {
    assertThat(measure.getMetric()).isEqualTo(metric);
    assertThat(measure.getValue()).isEqualTo(value);
  }

  private static org.sonar.api.resources.File mockSonarFile(String languageKey) {
    Language language = mock(Language.class);
    when(language.getKey()).thenReturn(languageKey);
    org.sonar.api.resources.File sonarFile = mock(org.sonar.api.resources.File.class);
    when(sonarFile.getLanguage()).thenReturn(language);
    return sonarFile;
  }

}
