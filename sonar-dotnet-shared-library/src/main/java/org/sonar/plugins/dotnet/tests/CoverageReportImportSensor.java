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
import java.util.Map;
import java.util.Set;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.batch.sensor.coverage.NewCoverage;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

/**
 * This class is responsible to handle all the C# and VB.NET code coverage reports (parse and report back to SonarQube).
 */
public class CoverageReportImportSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CoverageReportImportSensor.class);

  private final WildcardPatternFileProvider wildcardPatternFileProvider = new WildcardPatternFileProvider(new File("."), File.separator);
  private final CoverageConfiguration coverageConf;
  private final CoverageAggregator coverageAggregator;
  private final boolean isIntegrationTest;
  private final String languageKey;
  private final String languageName;

  public CoverageReportImportSensor(CoverageConfiguration coverageConf, CoverageAggregator coverageAggregator,
    String languageKey, String languageName, boolean isIntegrationTest) {
    this.coverageConf = coverageConf;
    this.coverageAggregator = coverageAggregator;
    this.isIntegrationTest = isIntegrationTest;
    this.languageKey = languageKey;
    this.languageName = languageName;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    if (this.isIntegrationTest) {
      descriptor.name("[Deprecated] " + this.languageName + " Integration Tests Coverage Report Import");
    } else {
      descriptor.name(this.languageName + " Tests Coverage Report Import");
    }
    descriptor.global();
    descriptor.onlyWhenConfiguration(c -> coverageAggregator.hasCoverageProperty(c::hasKey));

    descriptor.onlyOnLanguage(this.languageKey);
  }

  @Override
  public void execute(SensorContext context) {
    if (!coverageAggregator.hasCoverageProperty()) {
      LOG.debug("No coverage property. Skip Sensor");
      return;
    }

    if (this.isIntegrationTest) {
      LOG.warn("Starting with SonarQube 6.2 separation between Unit Tests and Integration Tests Coverage" +
        " reports is deprecated. Please move all reports specified from *.it.reportPaths into *.reportPaths.");
    }

    analyze(context, new Coverage());
  }

  void analyze(SensorContext context, Coverage coverage) {

    LOG.info("ANALYZE coverage with wildcardPatternFileProvider ." + File.separator);

    coverageAggregator.aggregate(wildcardPatternFileProvider, coverage);

    Set<String> coverageFiles = coverage.files();
    FileCountStatistics fileCountStatistics = new FileCountStatistics(coverageFiles.size());
    for (String filePath : coverageFiles) {
      FilePredicates p = context.fileSystem().predicates();
      InputFile inputFile = context.fileSystem().inputFile(p.hasAbsolutePath(filePath));

      if (inputFile == null) {
        fileCountStatistics.projectExcluded++;
        LOG.debug("The file '{}' is either excluded or outside of your solution folder therefore Code "
          + "Coverage will not be imported.", filePath);
        continue;
      }

      if (inputFile.type().equals(Type.TEST)) {
        fileCountStatistics.test++;
        // Do not log for test files to avoid pointless noise
        continue;
      }

      if (!coverageConf.languageKey().equals(inputFile.language())) {
        fileCountStatistics.otherLanguageExcluded++;
        continue;
      }

      fileCountStatistics.main++;
      boolean fileHasCoverage = false;

      NewCoverage newCoverage = context.newCoverage().onFile(inputFile);
      for (Map.Entry<Integer, Integer> entry : coverage.hits(filePath).entrySet()) {
        fileHasCoverage = true;
        newCoverage.lineHits(entry.getKey(), entry.getValue());
      }
      newCoverage.save();

      if (fileHasCoverage) {
        fileCountStatistics.mainWithCoverage++;
        LOG.debug("Found some coverage info for the file '{}'.", filePath);
      } else {
        LOG.debug("No coverage info found for the file '{}'.", filePath);
      }
    }

    if (fileCountStatistics.total != 0) {
      LOG.info(fileCountStatistics.toString());
      if (fileCountStatistics.mainWithCoverage == 0) {
        LOG.warn("The Code Coverage report doesn't contain any coverage data for the included files. For "
          + "troubleshooting hints, please refer to https://docs.sonarqube.org/x/CoBh");
      }
    }
  }

  private static class FileCountStatistics {

    private final int total;
    private int main = 0;
    private int mainWithCoverage = 0;
    private int test = 0;
    private int projectExcluded = 0;
    private int otherLanguageExcluded = 0;

    private FileCountStatistics(int total) {
      this.total = total;
    }

    @Override
    public String toString() {
      return "Coverage Report Statistics: " +
        total + " files, " +
        main + " main files, " +
        mainWithCoverage + " main files with coverage, " +
        test + " test files, " +
        projectExcluded + " project excluded files, " +
        otherLanguageExcluded + " other language files.";
    }

  }

}
