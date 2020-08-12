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

import java.io.File;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.DotNetPluginMetadata;

/**
 * This class is responsible to handle all the C# and VB.NET unit test results reports (parse and report back to SonarQube).
 */
public class UnitTestResultsImportSensor implements ProjectSensor {

  private static final Logger LOG = Loggers.get(UnitTestResultsImportSensor.class);

  private final WildcardPatternFileProvider wildcardPatternFileProvider = new WildcardPatternFileProvider(new File("."), File.separator);
  private final UnitTestResultsAggregator unitTestResultsAggregator;
  private final ProjectDefinition projectDef;
  private final String languageKey;
  private final String languageName;
  private final AnalysisWarnings analysisWarnings;

  public UnitTestResultsImportSensor(UnitTestResultsAggregator unitTestResultsAggregator, ProjectDefinition projectDef,
    DotNetPluginMetadata pluginMetadata, AnalysisWarnings analysisWarnings) {
    this(unitTestResultsAggregator, projectDef, pluginMetadata.languageKey(), pluginMetadata.languageName(), analysisWarnings);
  }

  public UnitTestResultsImportSensor(UnitTestResultsAggregator unitTestResultsAggregator, ProjectDefinition projectDef,
    String languageKey, String languageName, AnalysisWarnings analysisWarnings) {
    this.unitTestResultsAggregator = unitTestResultsAggregator;
    this.projectDef = projectDef;
    this.languageKey = languageKey;
    this.languageName = languageName;
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
    if (!unitTestResultsAggregator.hasUnitTestResultsProperty()) {
      LOG.debug("No unit test results property. Skip Sensor");
      return;
    }
    if (projectDef.getParent() == null) {
      analyze(context, new UnitTestResults());
    }
  }

  void analyze(SensorContext context, UnitTestResults unitTestResults) {
    try {
      saveTestMetrics(context, unitTestResults);
    } catch (Exception e) {
      LOG.warn("Could not import unit test report: '{}'", e.getMessage());
      analysisWarnings.addUnique(String.format("Could not import unit test report for '%s'. Please check the logs for more details.", languageName));
    }
  }

  private void saveTestMetrics(SensorContext context, UnitTestResults unitTestResults) {
    UnitTestResults aggregatedResults = unitTestResultsAggregator.aggregate(wildcardPatternFileProvider, unitTestResults);

    context.<Integer>newMeasure()
      .forMetric(CoreMetrics.TESTS)
      .on(context.project())
      .withValue(aggregatedResults.tests())
      .save();
    context.<Integer>newMeasure()
      .forMetric(CoreMetrics.TEST_ERRORS)
      .on(context.project())
      .withValue(aggregatedResults.errors())
      .save();
    context.<Integer>newMeasure()
      .forMetric(CoreMetrics.TEST_FAILURES)
      .on(context.project())
      .withValue(aggregatedResults.failures())
      .save();
    context.<Integer>newMeasure()
      .forMetric(CoreMetrics.SKIPPED_TESTS)
      .on(context.project())
      .withValue(aggregatedResults.skipped())
      .save();

    Long executionTime = aggregatedResults.executionTime();
    if (executionTime != null) {
      context.<Long>newMeasure()
        .forMetric(CoreMetrics.TEST_EXECUTION_TIME)
        .on(context.project())
        .withValue(executionTime)
        .save();
    }
  }
}
