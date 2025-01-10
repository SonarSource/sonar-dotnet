/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Predicate;
import org.sonar.api.config.Configuration;
import org.sonar.api.scanner.ScannerSide;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.sonarsource.dotnet.protobuf.SonarAnalyzer.MethodDeclarationsInfo;

/**
 * Aggregate the test results from different reports of potentially different tools (e.g. aggregate a NUnit report with a xUnit one and 3 Visual Studio ones).
 */
@ScannerSide
public class UnitTestResultsAggregator {

  private static final Logger LOG = LoggerFactory.getLogger(UnitTestResultsAggregator.class);

  private final UnitTestConfiguration unitTestConf;
  private final Configuration configuration;
  private final VisualStudioTestResultParser visualStudioTestResultsParser;
  private final NUnitTestResultsParser nUnitTestResultsParser;
  private final XUnitTestResultsParser xunitTestResultsParser;

  public UnitTestResultsAggregator(UnitTestConfiguration unitTestConf, Configuration configuration) {
    this(unitTestConf, configuration, new VisualStudioTestResultParser(), new NUnitTestResultsParser(), new XUnitTestResultsParser());
  }

  UnitTestResultsAggregator(
    UnitTestConfiguration unitTestConf,
    Configuration configuration,
    VisualStudioTestResultParser visualStudioTestResultsFileParser,
    NUnitTestResultsParser nUnitTestResultsParser,
    XUnitTestResultsParser xunitTestResultsParser) {
    this.unitTestConf = unitTestConf;
    this.configuration = configuration;
    this.visualStudioTestResultsParser = visualStudioTestResultsFileParser;
    this.nUnitTestResultsParser = nUnitTestResultsParser;
    this.xunitTestResultsParser = xunitTestResultsParser;
  }

  boolean hasUnitTestResultsProperty(Predicate<String> hasKeyPredicate) {
    return hasVisualStudioTestResultsFile(hasKeyPredicate)
      || hasNUnitTestResultsFile(hasKeyPredicate)
      || hasXUnitTestResultsFile(hasKeyPredicate);
  }

  boolean hasUnitTestResultsProperty() {
    return hasUnitTestResultsProperty(configuration::hasKey);
  }

  /**
   * New metrics aggregation (per file).
   */
  Map<String, UnitTestResults> aggregate(WildcardPatternFileProvider wildcardProvider, Collection<SonarAnalyzer.MethodDeclarationsInfo> methodDeclarations) {
    HashMap<String, String> fileMethodMap = computeMethodFileMap(methodDeclarations);
    var results = new HashMap<String, UnitTestResults>();
    if (hasVisualStudioTestResultsFile(configuration::hasKey)) {
      aggregate(wildcardProvider, configuration.getStringArray(unitTestConf.visualStudioTestResultsFilePropertyKey()), visualStudioTestResultsParser, fileMethodMap, results);
    }
    if (hasXUnitTestResultsFile(configuration::hasKey)) {
      aggregate(wildcardProvider, configuration.getStringArray(unitTestConf.xunitTestResultsFilePropertyKey()), xunitTestResultsParser, fileMethodMap, results);
    }
    if (hasNUnitTestResultsFile(configuration::hasKey)) {
      aggregate(wildcardProvider, configuration.getStringArray(unitTestConf.nunitTestResultsFilePropertyKey()), nUnitTestResultsParser, fileMethodMap, results);
    }
    return results;
  }

  private static void aggregate(
    WildcardPatternFileProvider wildcardPatternFileProvider,
    String[] reportFilePatterns,
    UnitTestResultParser parser,
    Map<String, String> methodFileMap,
    Map<String, UnitTestResults> unitTestResultsMap) {
    for (String reportPathPattern : reportFilePatterns) {
      if (!reportPathPattern.isEmpty()) {
        for (File reportFile : wildcardPatternFileProvider.listFiles(reportPathPattern)) {
          try {
            parser.parse(reportFile, unitTestResultsMap, methodFileMap);
          } catch (Exception e) {
            LOG.warn("Could not import unit test report '{}': {}", reportFile, e.getMessage());
          }
        }
      }
    }
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

  private static HashMap<String, String> computeMethodFileMap(Collection<SonarAnalyzer.MethodDeclarationsInfo> methodDeclarations) {
    var results = new HashMap<String, String>();
    for (MethodDeclarationsInfo methodDeclaration : methodDeclarations) {
      String assemblyName = methodDeclaration.getAssemblyName();
      String filePath = methodDeclaration.getFilePath();
      for (var methodDeclarationInfo : methodDeclaration.getMethodDeclarationsList()) {
        String key = assemblyName.trim() + "." + methodDeclarationInfo.getTypeName().trim() + "." + methodDeclarationInfo.getMethodName().trim();
        results.put(key, filePath);
      }
    }
    return results;
  }
}
