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
package org.sonar.plugins.dotnet.tests;

import java.io.IOException;
import java.util.function.Predicate;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;
import org.sonarsource.dotnet.shared.plugins.DotNetPluginMetadata;

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

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void coverage() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    new UnitTestResultsImportSensor(unitTestResultsAggregator, metadata, analysisWarnings).describe(new DefaultSensorDescriptor());
    SensorContext sensorContext = mock(SensorContext.class);
    new UnitTestResultsImportSensor(unitTestResultsAggregator, metadata, analysisWarnings).execute(sensorContext);
    verifyZeroInteractions(sensorContext);
  }

  @Test
  public void should_not_save_metrics_with_empty_results() throws Exception {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    UnitTestResults results = new UnitTestResults();
    results.add(42, 1, 2, 3, null);
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(true);
    when(aggregator.aggregate(any())).thenReturn(results);
    SensorContextTester context = SensorContextTester.create(temp.newFolder());
    when(aggregator.aggregate(any())).thenReturn(results);

    new UnitTestResultsImportSensor(aggregator, metadata, analysisWarnings).execute(context);

    assertThat(context.measures("projectKey"))
      .extracting("metric.key", "value")
      .containsOnly(
        tuple(CoreMetrics.TESTS_KEY, 42),
        tuple(CoreMetrics.SKIPPED_TESTS_KEY, 1),
        tuple(CoreMetrics.TEST_FAILURES_KEY, 2),
        tuple(CoreMetrics.TEST_ERRORS_KEY, 3));
  }

  @Test
  public void describe_execute_only_when_key_present() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);

    Configuration configWithKey = mock(Configuration.class);
    when(configWithKey.hasKey("expectedKey")).thenReturn(true);

    Configuration configWithoutKey = mock(Configuration.class);

    when(unitTestResultsAggregator.hasUnitTestResultsProperty(any(Predicate.class))).thenAnswer((invocationOnMock) -> {
      Predicate<String> pr = invocationOnMock.getArgument(0);
      return pr.test("expectedKey");
    });
    DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

    new UnitTestResultsImportSensor(unitTestResultsAggregator, metadata, analysisWarnings).describe(descriptor);

    assertThat(descriptor.configurationPredicate()).accepts(configWithKey);
    assertThat(descriptor.configurationPredicate()).rejects(configWithoutKey);
  }

  @Test
  public void describe_only_on_language() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

    new UnitTestResultsImportSensor(unitTestResultsAggregator, metadata, analysisWarnings).describe(descriptor);

    assertThat(descriptor.languages()).containsOnly("cs");
  }

  @Test
  public void isProjectSensor() {
    assertThat(ProjectSensor.class.isAssignableFrom(UnitTestResultsImportSensor.class)).isTrue();
  }

  @Test
  public void import_two_reports_for_same_project_should_not_throw() throws Exception {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata cSharpMetadata = mockCSharpMetadata();
    DotNetPluginMetadata vbNetMetadata = mock(DotNetPluginMetadata.class);
    when(vbNetMetadata.languageKey()).thenReturn("vb");
    when(vbNetMetadata.languageName()).thenReturn("VB.NET");
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    SensorContextTester context = SensorContextTester.create(temp.newFolder());
    UnitTestResults results = mock(UnitTestResults.class);
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(true);
    when(aggregator.aggregate(any())).thenReturn(results);

    new UnitTestResultsImportSensor(aggregator, cSharpMetadata, analysisWarnings).execute(context);
    new UnitTestResultsImportSensor(aggregator, vbNetMetadata, analysisWarnings).execute(context);

    assertThat(logTester.logs(LoggerLevel.WARN)).containsExactly("Could not import unit test report: 'Can not add the same measure twice'");
    verify(analysisWarnings).addUnique("Could not import unit test report for 'VB.NET'. Please check the logs for more details.");
  }

  @Test
  public void execute_saves_metrics() throws IOException {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    UnitTestResults results = new UnitTestResults();
    results.add(42, 1, 2, 3, 321L);
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(true);
    when(aggregator.aggregate(any())).thenReturn(results);
    SensorContextTester context = SensorContextTester.create(temp.newFolder());

    UnitTestResultsImportSensor sensor = new UnitTestResultsImportSensor(aggregator, metadata, analysisWarnings);
    sensor.execute(context);

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
  public void execute_warns_when_no_key_is_present() throws IOException {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    DotNetPluginMetadata metadata = mockCSharpMetadata();
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(false);

    UnitTestResultsImportSensor sensor = new UnitTestResultsImportSensor(aggregator, metadata, null);
    sensor.execute(null);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("No unit test results property. Skip Sensor");
  }

  private DotNetPluginMetadata mockCSharpMetadata() {
    DotNetPluginMetadata metadata = mock(DotNetPluginMetadata.class);
    when(metadata.languageKey()).thenReturn("cs");
    when(metadata.languageName()).thenReturn("C#");
    return metadata;
  }
}
