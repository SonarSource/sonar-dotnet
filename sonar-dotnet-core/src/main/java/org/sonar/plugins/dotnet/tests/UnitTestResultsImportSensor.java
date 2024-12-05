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
import java.io.Serializable;
import java.util.Map;
import org.sonar.api.batch.fs.InputComponent;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.shared.plugins.MethodDeclarationsCollector;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.SensorContextUtils;

/**
 * This class is responsible to handle all the C# and VB.NET unit test results reports (parse and report back to SonarQube).
 */
public class UnitTestResultsImportSensor implements ProjectSensor {

  private static final Logger LOG = LoggerFactory.getLogger(UnitTestResultsImportSensor.class);

  private final WildcardPatternFileProvider wildcardPatternFileProvider = new WildcardPatternFileProvider(new File("."));
  private final UnitTestResultsAggregator unitTestResultsAggregator;
  private final String languageKey;
  private final String languageName;
  private final AnalysisWarnings analysisWarnings;
  private final MethodDeclarationsCollector collector;

  public UnitTestResultsImportSensor(
    MethodDeclarationsCollector collector,
    UnitTestResultsAggregator unitTestResultsAggregator,
    PluginMetadata pluginMetadata,
    AnalysisWarnings analysisWarnings) {
    this.collector = collector;
    this.unitTestResultsAggregator = unitTestResultsAggregator;
    this.languageKey = pluginMetadata.languageKey();
    this.languageName = pluginMetadata.languageName();
    this.analysisWarnings = analysisWarnings;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Unit Test Results Import", this.languageName);
    descriptor.name(name);
    descriptor.onlyOnLanguage(this.languageKey);
    descriptor.onlyWhenConfiguration(c -> unitTestResultsAggregator.hasUnitTestResultsProperty(c::hasKey));
  }

  @Override
  public void execute(SensorContext context) {
    if (unitTestResultsAggregator.hasUnitTestResultsProperty()) {
      try {
        addTestMetrics(context);
      } catch (Exception e) {
        LOG.warn("Could not import unit test report: '{}'", e.getMessage());
        analysisWarnings.addUnique(String.format("Could not import unit test report for '%s'. Please check the logs for more details.", languageName));
      }
    } else {
      LOG.debug("No unit test results property. Skip Sensor");
    }
  }

  private void addTestMetrics(SensorContext context) {
    var methodDeclarations = collector.getMethodDeclarations();
    var aggregatedResultsPerFile = unitTestResultsAggregator.aggregate(wildcardPatternFileProvider, methodDeclarations);
    addMeasures(context, aggregatedResultsPerFile);
  }

  private static void addMeasures(SensorContext context, Map<String, UnitTestResults> aggregatedResultsPerFile) {
    for (var entry : aggregatedResultsPerFile.entrySet()) {
      var inputFile = SensorContextUtils.toInputFile(context.fileSystem(), entry.getKey());
      if (inputFile != null) {
        addMeasure(context, inputFile, CoreMetrics.TESTS, entry.getValue().tests());
        addMeasure(context, inputFile, CoreMetrics.TEST_ERRORS, entry.getValue().errors());
        addMeasure(context, inputFile, CoreMetrics.TEST_FAILURES, entry.getValue().failures());
        addMeasure(context, inputFile, CoreMetrics.SKIPPED_TESTS, entry.getValue().skipped());
        if (entry.getValue().executionTime() != null) {
          addMeasure(context, inputFile, CoreMetrics.TEST_EXECUTION_TIME, entry.getValue().executionTime());
        }
      }
    }
  }

  private static <T extends Serializable> void addMeasure(SensorContext context, InputComponent inputComponent, Metric<T> metric, T value) {
    context.<T>newMeasure().forMetric(metric).on(inputComponent).withValue(value).save();
  }
}
