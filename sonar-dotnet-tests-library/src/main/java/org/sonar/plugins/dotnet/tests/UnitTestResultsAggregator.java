/*
 * SonarQube .NET Tests Library
 * Copyright (C) 2014-2018 SonarSource SA
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
import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;

@ScannerSide
public class UnitTestResultsAggregator {

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

  List<UnitTestResult> aggregate(WildcardPatternFileProvider wildcardPatternFileProvider) {

    List<UnitTestResult> unitTestResults = new ArrayList<>();

    if (hasVisualStudioTestResultsFile(configuration::hasKey)) {
      unitTestResults.addAll(aggregate(wildcardPatternFileProvider, configuration.getStringArray(unitTestConf.visualStudioTestResultsFilePropertyKey()), visualStudioTestResultsFileParser));
    }

    if (hasNUnitTestResultsFile(configuration::hasKey)) {
      unitTestResults.addAll(aggregate(wildcardPatternFileProvider, configuration.getStringArray(unitTestConf.nunitTestResultsFilePropertyKey()), nunitTestResultsFileParser));
    }

    if (hasXUnitTestResultsFile(configuration::hasKey)) {
      unitTestResults.addAll(aggregate(wildcardPatternFileProvider, configuration.getStringArray(unitTestConf.xunitTestResultsFilePropertyKey()), xunitTestResultsFileParser));
    }

    return unitTestResults;
  }

  private static List<UnitTestResult> aggregate(WildcardPatternFileProvider wildcardPatternFileProvider, String[] reportPaths, UnitTestResultsParser parser) {

    List<UnitTestResult> aggregatedResults = new ArrayList<>();

    for (String reportPathPattern : reportPaths) {
      if (!reportPathPattern.isEmpty()) {
        for (File reportFile : wildcardPatternFileProvider.listFiles(reportPathPattern)) {
          aggregatedResults.addAll(parser.apply(reportFile));
        }
      }
    }

    return aggregatedResults;
  }

}
