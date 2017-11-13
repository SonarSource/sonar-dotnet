/*
 * SonarQube .NET Tests Library
 * Copyright (C) 2014-2017 SonarSource SA
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
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;

@ScannerSide
public class CoverageAggregator {

  private final CoverageConfiguration coverageConf;
  private final Configuration configuration;
  private final CoverageCache coverageCache;
  private final NCover3ReportParser ncover3ReportParser;
  private final OpenCoverReportParser openCoverReportParser;
  private final DotCoverReportsAggregator dotCoverReportsAggregator;
  private final VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser;

  public CoverageAggregator(CoverageConfiguration coverageConf, Configuration configuration) {
    this(coverageConf, configuration,
      new CoverageCache(),
      new NCover3ReportParser(),
      new OpenCoverReportParser(),
      new DotCoverReportsAggregator(new DotCoverReportParser()),
      new VisualStudioCoverageXmlReportParser());
  }

  public CoverageAggregator(CoverageConfiguration coverageConf, Configuration configuration,
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
        for (File reportFile : wildcardPatternFileProvider.listFiles(reportPathPattern)) {
          aggregatedCoverage.mergeWith(coverageCache.readCoverageFromCacheOrParse(parser, reportFile));
        }
      }
    }
  }

}
