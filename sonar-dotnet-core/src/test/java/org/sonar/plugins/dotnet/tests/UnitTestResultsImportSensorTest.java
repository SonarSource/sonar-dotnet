/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.function.Predicate;
import org.assertj.core.api.Condition;
import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;
import org.sonarsource.dotnet.shared.plugins.MethodDeclarationsCollector;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.groups.Tuple.tuple;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class UnitTestResultsImportSensorTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void should_not_save_metrics_with_empty_results() throws Exception {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    PluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    UnitTestResults results = new UnitTestResults();
    results.add(42, 1, 2, 3, null);
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(true);
    when(aggregator.aggregateOld(any())).thenReturn(results);
    SensorContextTester context = SensorContextTester.create(temp.newFolder());
    when(aggregator.aggregateOld(any())).thenReturn(results);
    Condition<Tuple> executionTimeMetric = new Condition<>(x -> x.toList().get(0) == CoreMetrics.TEST_EXECUTION_TIME_KEY, CoreMetrics.TEST_EXECUTION_TIME_KEY);

    new UnitTestResultsImportSensor(new MethodDeclarationsCollector(), aggregator, metadata, analysisWarnings).execute(context);

    assertThat(context.measures("projectKey"))
      .extracting("metric.key", "value")
      .containsOnly(
        tuple(CoreMetrics.TESTS_KEY, 42),
        tuple(CoreMetrics.SKIPPED_TESTS_KEY, 1),
        tuple(CoreMetrics.TEST_FAILURES_KEY, 2),
        tuple(CoreMetrics.TEST_ERRORS_KEY, 3))
      .doNotHave(executionTimeMetric);
  }

  @Test
  public void describe_execute_only_when_key_present() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    PluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);

    Configuration configWithKey = mock(Configuration.class);
    when(configWithKey.hasKey("expectedKey")).thenReturn(true);

    Configuration configWithoutKey = mock(Configuration.class);

    when(unitTestResultsAggregator.hasUnitTestResultsProperty(any(Predicate.class))).thenAnswer((invocationOnMock) -> {
      Predicate<String> pr = invocationOnMock.getArgument(0);
      return pr.test("expectedKey");
    });
    DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

    new UnitTestResultsImportSensor(new MethodDeclarationsCollector(), unitTestResultsAggregator, metadata, analysisWarnings).describe(descriptor);

    assertThat(descriptor.configurationPredicate()).accepts(configWithKey);
    assertThat(descriptor.configurationPredicate()).rejects(configWithoutKey);
  }

  @Test
  public void describe_only_on_language() {
    UnitTestResultsAggregator unitTestResultsAggregator = mock(UnitTestResultsAggregator.class);
    PluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

    new UnitTestResultsImportSensor(new MethodDeclarationsCollector(), unitTestResultsAggregator, metadata, analysisWarnings).describe(descriptor);

    assertThat(descriptor.languages()).containsOnly("cs");
  }

  @Test
  public void isProjectSensor() {
    assertThat(ProjectSensor.class.isAssignableFrom(UnitTestResultsImportSensor.class)).isTrue();
  }

  @Test
  public void import_two_reports_for_same_project_should_not_throw() throws Exception {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    PluginMetadata cSharpMetadata = mockCSharpMetadata();
    PluginMetadata vbNetMetadata = mockVbNetMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    SensorContextTester context = SensorContextTester.create(temp.newFolder());
    UnitTestResults results = mock(UnitTestResults.class);
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(true);
    when(aggregator.aggregateOld(any())).thenReturn(results);

    var methodDeclarationsCollector = new MethodDeclarationsCollector();
    new UnitTestResultsImportSensor(methodDeclarationsCollector, aggregator, cSharpMetadata, analysisWarnings).execute(context);
    new UnitTestResultsImportSensor(methodDeclarationsCollector, aggregator, vbNetMetadata, analysisWarnings).execute(context);

    assertThat(logTester.logs(Level.WARN)).containsExactly("Could not import unit test report: 'Can not add the same measure twice'");
    verify(analysisWarnings).addUnique("Could not import unit test report for 'VB.NET'. Please check the logs for more details.");
  }

  @Test
  public void execute_saves_metrics() throws IOException {
    UnitTestResultsAggregator aggregator = mock(UnitTestResultsAggregator.class);
    PluginMetadata metadata = mockCSharpMetadata();
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(true);

    UnitTestResults results = new UnitTestResults();
    results.add(42, 1, 2, 3, 321L);

    var fileResults = new HashMap<String, UnitTestResults>();
    var tempFolder = temp.newFolder();
    var file = File.createTempFile("path", ".cs", tempFolder);
    fileResults.put(file.getName(), results);
    when(aggregator.aggregate(any(), any())).thenReturn(fileResults);
    when(aggregator.aggregateOld(any())).thenReturn(results);
    SensorContextTester context = SensorContextTester.create(tempFolder);
    context.fileSystem().add(new TestInputFileBuilder("projectKey", file.getName())
      .setLanguage("cs")
      .setType(InputFile.Type.MAIN)
      .build());

    UnitTestResultsImportSensor sensor = new UnitTestResultsImportSensor(new MethodDeclarationsCollector(), aggregator, metadata, analysisWarnings);
    sensor.execute(context);

    // Project metrics
    assertThat(context.measures("projectKey"))
      .extracting("metric.key", "value")
      .containsOnly(
        tuple(CoreMetrics.TESTS_KEY, 42),
        tuple(CoreMetrics.SKIPPED_TESTS_KEY, 1),
        tuple(CoreMetrics.TEST_FAILURES_KEY, 2),
        tuple(CoreMetrics.TEST_ERRORS_KEY, 3),
        tuple(CoreMetrics.TEST_EXECUTION_TIME_KEY, 321L));

    // File metrics
    assertThat(context.measures("projectKey:" + file.getName()))
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
    PluginMetadata metadata = mockCSharpMetadata();
    when(aggregator.hasUnitTestResultsProperty()).thenReturn(false);

    UnitTestResultsImportSensor sensor = new UnitTestResultsImportSensor(new MethodDeclarationsCollector(), aggregator, metadata, null);
    sensor.execute(null);

    assertThat(logTester.logs(Level.DEBUG)).containsExactly("No unit test results property. Skip Sensor");
  }

  private PluginMetadata mockCSharpMetadata() {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageKey()).thenReturn("cs");
    when(metadata.languageName()).thenReturn("C#");
    return metadata;
  }

  private PluginMetadata mockVbNetMetadata() {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageKey()).thenReturn("vb");
    when(metadata.languageName()).thenReturn("VB.NET");
    return metadata;
  }
}
