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

import java.io.File;
import java.util.Set;
import java.util.function.Predicate;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.config.Configuration;
import org.sonar.api.internal.google.common.annotations.VisibleForTesting;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

/**
 * Aggregate the coverage results from different reports of potentially different tools (e.g. aggregate a NCover 3 report with a DotCover one and 3 Visual Studio ones).
 */
@ScannerSide
public class CoverageAggregator {

  private static final Logger LOG = Loggers.get(CoverageAggregator.class);

  private final CoverageConfiguration coverageConf;
  private final Configuration configuration;
  private final CoverageCache coverageCache;
  private final NCover3ReportParser ncover3ReportParser;
  private final OpenCoverReportParser openCoverReportParser;
  private final DotCoverReportsAggregator dotCoverReportsAggregator;
  private final VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser;

  public CoverageAggregator(CoverageConfiguration coverageConf, Configuration configuration, FileSystem fs) {

    Predicate<String> isSupportedLanguage = absolutePath -> fs.hasFiles(fs.predicates().and(fs.predicates().hasAbsolutePath(absolutePath),
      fs.predicates().hasLanguage(coverageConf.languageKey())));

    this.coverageConf = coverageConf;
    this.configuration = configuration;
    this.coverageCache = new CoverageCache();
    this.ncover3ReportParser = new NCover3ReportParser(isSupportedLanguage);
    this.openCoverReportParser = new OpenCoverReportParser(isSupportedLanguage);
    this.dotCoverReportsAggregator = new DotCoverReportsAggregator(new DotCoverReportParser(isSupportedLanguage));
    this.visualStudioCoverageXmlReportParser = new VisualStudioCoverageXmlReportParser(isSupportedLanguage);
  }

  @VisibleForTesting
  CoverageAggregator(CoverageConfiguration coverageConf, Configuration configuration,
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
          LOG.warn("Could not find any coverage report file matching the pattern '{}'.", reportPathPattern);
        } else {
          for (File reportFile : filesMatchingPattern) {
            aggregatedCoverage.mergeWith(coverageCache.readCoverageFromCacheOrParse(parser, reportFile));
          }
        }
      }
    }
  }

}
