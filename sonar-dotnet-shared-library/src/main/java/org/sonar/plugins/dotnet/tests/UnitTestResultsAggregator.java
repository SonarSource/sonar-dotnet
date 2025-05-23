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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.util.function.Predicate;
import org.sonar.api.config.Configuration;
import org.sonar.api.scanner.ScannerSide;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Aggregate the test results from different reports of potentially different tools (e.g. aggregate a NUnit report with a xUnit one and 3 Visual Studio ones).
 */
@ScannerSide
public class UnitTestResultsAggregator {

  private static final Logger LOG = LoggerFactory.getLogger(UnitTestResultsAggregator.class);

  private final UnitTestConfiguration unitTestConf;
  private final Configuration configuration;
  private final VisualStudioTestResultsFileParser visualStudioTestResultsFileParser;
  private final NUnitTestResultsFileParser nunitTestResultsFileParser;
  private final XUnitTestResultsFileParser xunitTestResultsFileParser;

  public UnitTestResultsAggregator(UnitTestConfiguration unitTestConf, Configuration configuration) {
    this(unitTestConf, configuration, new VisualStudioTestResultsFileParser(), new NUnitTestResultsFileParser(), new XUnitTestResultsFileParser());
  }

  UnitTestResultsAggregator(UnitTestConfiguration unitTestConf, Configuration configuration,
    VisualStudioTestResultsFileParser visualStudioTestResultsFileParser,
    NUnitTestResultsFileParser nunitTestResultsFileParser,
    XUnitTestResultsFileParser xunitTestResultsFileParser) {
    this.unitTestConf = unitTestConf;
    this.configuration = configuration;
    this.visualStudioTestResultsFileParser = visualStudioTestResultsFileParser;
    this.nunitTestResultsFileParser = nunitTestResultsFileParser;
    this.xunitTestResultsFileParser = xunitTestResultsFileParser;
  }

  boolean hasUnitTestResultsProperty(Predicate<String> hasKeyPredicate) {
    return hasVisualStudioTestResultsFile(hasKeyPredicate)
      || hasNUnitTestResultsFile(hasKeyPredicate)
      || hasXUnitTestResultsFile(hasKeyPredicate);
  }

  boolean hasUnitTestResultsProperty() {
    return hasUnitTestResultsProperty(configuration::hasKey);
  }

  private boolean hasVisualStudioTestResultsFile(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(unitTestConf.visualStudioTestResultsFilePropertyKey());
  }

  private boolean hasNUnitTestResultsFile(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(unitTestConf.nunitTestResultsFilePropertyKey());
  }

  private boolean hasXUnitTestResultsFile(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(unitTestConf.xunitTestResultsFilePropertyKey());
  }

  UnitTestResults aggregate(WildcardPatternFileProvider wildcardPatternFileProvider) {
    UnitTestResults results = new UnitTestResults();

    if (hasVisualStudioTestResultsFile(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(unitTestConf.visualStudioTestResultsFilePropertyKey()), visualStudioTestResultsFileParser, results);
    }

    if (hasNUnitTestResultsFile(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(unitTestConf.nunitTestResultsFilePropertyKey()), nunitTestResultsFileParser, results);
    }

    if (hasXUnitTestResultsFile(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(unitTestConf.xunitTestResultsFilePropertyKey()), xunitTestResultsFileParser, results);
    }

    return results;
  }

  private static void aggregate(WildcardPatternFileProvider wildcardPatternFileProvider, String[] reportPaths, UnitTestResultsParser parser, UnitTestResults unitTestResults) {
    for (String reportPathPattern : reportPaths) {
      if (!reportPathPattern.isEmpty()) {
        for (File reportFile : wildcardPatternFileProvider.listFiles(reportPathPattern)) {
          try {
            parser.accept(reportFile, unitTestResults);
          } catch (Exception e) {
            LOG.warn("Could not import unit test report '{}': {}", reportFile, e.getMessage());
          }
        }
      }
    }
  }

}
