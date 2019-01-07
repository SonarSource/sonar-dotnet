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
package org.sonar.plugins.dotnet.tests;

import java.util.function.Predicate;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.Mockito;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.measures.CoreMetrics;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.groups.Tuple.tuple;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.mockito.Mockito.when;

public class UnitTestResultsImportSensorTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Test
  public void coverage() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    new UnitTestResultsImportSensor(unitTestResultsAggregator, ProjectDefinition.create(), "cs", "C#")
      .describe(new DefaultSensorDescriptor());
    SensorContext sensorContext = mock(SensorContext.class);
    new UnitTestResultsImportSensor(unitTestResultsAggregator, ProjectDefinition.create(), "cs", "C#")
      .execute(sensorContext);
    verifyZeroInteractions(sensorContext);
    when(unitTestResultsAggregator.hasUnitTestResultsProperty()).thenReturn(true);
    ProjectDefinition sub = ProjectDefinition.create();
    ProjectDefinition.create().addSubProject(sub);
    new UnitTestResultsImportSensor(unitTestResultsAggregator, sub, "cs", "C#")
      .execute(sensorContext);
    verifyZeroInteractions(sensorContext);
  }

  @Test
  public void analyze() throws Exception {
    UnitTestResults results = mock(UnitTestResults.class);
    when(results.tests()).thenReturn(42);
    when(results.skipped()).thenReturn(1);
    when(results.failures()).thenReturn(2);
    when(results.errors()).thenReturn(3);
    when(results.executionTime()).thenReturn(321L);

    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    SensorContextTester context = SensorContextTester.create(temp.newFolder());

    when(unitTestResultsAggregator.aggregate(Mockito.any(WildcardPatternFileProvider.class), Mockito.any(UnitTestResults.class)))
      .thenReturn(results);

    new UnitTestResultsImportSensor(unitTestResultsAggregator, ProjectDefinition.create(), "cs", "C#")
      .analyze(context, results);

    verify(unitTestResultsAggregator).aggregate(Mockito.any(WildcardPatternFileProvider.class), Mockito.eq(results));

    assertThat(context.measures("projectKey"))
      .extracting("metric.key", "value")
      .containsOnly(
        tuple(CoreMetrics.TESTS_KEY, 42),
        tuple(CoreMetrics.SKIPPED_TESTS_KEY, 1),
        tuple(CoreMetrics.TEST_FAILURES_KEY, 2),
        tuple(CoreMetrics.TEST_ERRORS_KEY, 3),
        tuple(CoreMetrics.TEST_EXECUTION_TIME_KEY, 321L));
  }

  @Test
  public void should_not_save_metrics_with_empty_results() throws Exception {
    SensorContextTester context = SensorContextTester.create(temp.newFolder());

    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    UnitTestResults results = mock(UnitTestResults.class);
    when(results.tests()).thenReturn(0);
    when(results.skipped()).thenReturn(1);
    when(results.failures()).thenReturn(2);
    when(results.errors()).thenReturn(3);
    when(results.executionTime()).thenReturn(null);
    when(unitTestResultsAggregator.aggregate(Mockito.any(WildcardPatternFileProvider.class), Mockito.any(UnitTestResults.class))).thenReturn(results);

    new UnitTestResultsImportSensor(unitTestResultsAggregator, ProjectDefinition.create(), "cs", "C#")
      .analyze(context, results);

    verify(unitTestResultsAggregator).aggregate(Mockito.any(WildcardPatternFileProvider.class), Mockito.eq(results));

    assertThat(context.measures("projectKey"))
      .extracting("metric.key", "value")
      .containsOnly(
        tuple(CoreMetrics.TESTS_KEY, 0),
        tuple(CoreMetrics.SKIPPED_TESTS_KEY, 1),
        tuple(CoreMetrics.TEST_FAILURES_KEY, 2),
        tuple(CoreMetrics.TEST_ERRORS_KEY, 3));
  }

  @Test
  public void describe_execute_only_when_key_present() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);

    Configuration configWithKey = mock(Configuration.class);
    when(configWithKey.hasKey("expectedKey")).thenReturn(true);

    Configuration configWithoutKey = mock(Configuration.class);

    when(unitTestResultsAggregator.hasUnitTestResultsProperty(any(Predicate.class))).thenAnswer((invocationOnMock) -> {
      Predicate<String> pr = invocationOnMock.getArgument(0);
      return pr.test("expectedKey");
    });
    DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

    new UnitTestResultsImportSensor(unitTestResultsAggregator, ProjectDefinition.create(), "cs", "C#")
      .describe(descriptor);

    assertThat(descriptor.configurationPredicate()).accepts(configWithKey);
    assertThat(descriptor.configurationPredicate()).rejects(configWithoutKey);
  }

  @Test
  public void describe_is_global_and_only_on_language() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

    new UnitTestResultsImportSensor(unitTestResultsAggregator, ProjectDefinition.create(), "cs", "C#")
      .describe(descriptor);

    assertThat(descriptor.isGlobal()).isTrue();
    assertThat(descriptor.languages()).containsOnly("cs");
  }

}
