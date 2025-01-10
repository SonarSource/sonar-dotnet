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
package org.sonar.plugins.dotnet.tests.coverage;

import java.io.File;
import java.util.Set;
import java.util.function.Predicate;
import org.sonar.api.config.Configuration;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.scanner.ScannerSide;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.dotnet.tests.ScannerFileService;
import org.sonar.plugins.dotnet.tests.WildcardPatternFileProvider;

/**
 * Aggregate the coverage results from different reports of potentially different tools (e.g. aggregate a NCover 3 report with a DotCover one and 3 Visual Studio ones).
 */
@ScannerSide
public class CoverageAggregator {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageAggregator.class);

  private final CoverageConfiguration coverageConf;
  private final Configuration configuration;
  private final CoverageCache coverageCache;
  private final NCover3ReportParser ncover3ReportParser;
  private final OpenCoverReportParser openCoverReportParser;
  private final DotCoverReportsAggregator dotCoverReportsAggregator;
  private final VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser;

  public CoverageAggregator(CoverageConfiguration coverageConf,
                            Configuration configuration,
                            ScannerFileService fileService,
                            AnalysisWarnings analysisWarnings) {
    this.coverageConf = coverageConf;
    this.configuration = configuration;
    this.coverageCache = new CoverageCache();
    this.ncover3ReportParser = new NCover3ReportParser(fileService, analysisWarnings);
    this.openCoverReportParser = new OpenCoverReportParser(fileService);
    this.dotCoverReportsAggregator = new DotCoverReportsAggregator(new DotCoverReportParser(fileService));
    this.visualStudioCoverageXmlReportParser = new VisualStudioCoverageXmlReportParser(fileService);
  }

  // visible for testing
  CoverageAggregator(CoverageConfiguration coverageConf,
                     Configuration configuration,
                     CoverageCache coverageCache,
                     NCover3ReportParser ncover3ReportParser,
                     OpenCoverReportParser openCoverReportParser,
                     DotCoverReportsAggregator dotCoverReportsAggregator,
                     VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser) {

    this.coverageConf = coverageConf;
    this.configuration = configuration;
    this.coverageCache = coverageCache;
    this.ncover3ReportParser = ncover3ReportParser;
    this.openCoverReportParser = openCoverReportParser;
    this.dotCoverReportsAggregator = dotCoverReportsAggregator;
    this.visualStudioCoverageXmlReportParser = visualStudioCoverageXmlReportParser;
  }

  boolean hasCoverageProperty() {
    return hasCoverageProperty(configuration::hasKey);
  }

   // visible for testing
  boolean hasCoverageProperty(Predicate<String> hasKeyPredicate) {
    return hasNCover3ReportPaths(hasKeyPredicate)
      || hasOpenCoverReportPaths(hasKeyPredicate)
      || hasDotCoverReportPaths(hasKeyPredicate)
      || hasVisualStudioCoverageXmlReportPaths(hasKeyPredicate);
  }

  private boolean hasNCover3ReportPaths(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(coverageConf.ncover3PropertyKey());
  }

  private boolean hasOpenCoverReportPaths(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(coverageConf.openCoverPropertyKey());
  }

  private boolean hasDotCoverReportPaths(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(coverageConf.dotCoverPropertyKey());
  }

  private boolean hasVisualStudioCoverageXmlReportPaths(Predicate<String> hasKeyPredicate) {
    return hasKeyPredicate.test(coverageConf.visualStudioCoverageXmlPropertyKey());
  }

  Coverage aggregate(WildcardPatternFileProvider wildcardPatternFileProvider, Coverage coverage) {
    if (hasNCover3ReportPaths(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(coverageConf.ncover3PropertyKey()), ncover3ReportParser, coverage);
    }

    if (hasOpenCoverReportPaths(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(coverageConf.openCoverPropertyKey()), openCoverReportParser, coverage);
    }

    if (hasDotCoverReportPaths(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(coverageConf.dotCoverPropertyKey()), dotCoverReportsAggregator, coverage);
    }

    if (hasVisualStudioCoverageXmlReportPaths(configuration::hasKey)) {
      aggregate(wildcardPatternFileProvider, configuration.getStringArray(coverageConf.visualStudioCoverageXmlPropertyKey()), visualStudioCoverageXmlReportParser, coverage);
    }

    return coverage;
  }

  private void aggregate(WildcardPatternFileProvider wildcardPatternFileProvider, String[] reportPaths, CoverageParser parser, Coverage aggregatedCoverage) {
    for (String reportPathPattern : reportPaths) {
      if (!reportPathPattern.isEmpty()) {
        Set<File> filesMatchingPattern = wildcardPatternFileProvider.listFiles(reportPathPattern);
        if (filesMatchingPattern.isEmpty()) {
          LOG.warn("Could not find any coverage report file matching the pattern '{}'. Troubleshooting guide: https://community.sonarsource.com/t/37151", reportPathPattern);
        } else {
          mergeParsedCoverageWithAggregatedCoverage(aggregatedCoverage, parser, filesMatchingPattern);
        }
      }
    }
  }

  private void mergeParsedCoverageWithAggregatedCoverage(Coverage aggregatedCoverage, CoverageParser parser, Set<File> filesMatchingPattern) {
    for (File reportFile : filesMatchingPattern) {
      try {
        aggregatedCoverage.mergeWith(coverageCache.readCoverageFromCacheOrParse(parser, reportFile));
      } catch (Exception e) {
        LOG.warn("Could not import coverage report '{}' because '{}'. Troubleshooting guide: https://community.sonarsource.com/t/37151", reportFile, e.getMessage());
      }
    }
  }

}
