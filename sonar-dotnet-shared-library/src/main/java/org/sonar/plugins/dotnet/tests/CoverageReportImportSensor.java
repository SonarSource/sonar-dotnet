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
import java.util.Map;
import java.util.Set;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.batch.sensor.coverage.NewCoverage;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

/**
 * This class is responsible to handle all the C# and VB.NET code coverage reports (parse and report back to SonarQube).
 */
public class CoverageReportImportSensor implements ProjectSensor {

  // visible for testing
  static final File BASE_DIR = new File(".");

  private static final Logger LOG = LoggerFactory.getLogger(CoverageReportImportSensor.class);

  private final WildcardPatternFileProvider wildcardPatternFileProvider = new WildcardPatternFileProvider(BASE_DIR);
  private final CoverageConfiguration coverageConf;
  private final CoverageAggregator coverageAggregator;
  private final String languageKey;
  private final String languageName;

  public CoverageReportImportSensor(CoverageConfiguration coverageConf, CoverageAggregator coverageAggregator,
                                    String languageKey, String languageName) {
    this.coverageConf = coverageConf;
    this.coverageAggregator = coverageAggregator;
    this.languageKey = languageKey;
    this.languageName = languageName;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(this.languageName + " Tests Coverage Report Import");
    descriptor.onlyWhenConfiguration(c -> coverageAggregator.hasCoverageProperty(c::hasKey));
    descriptor.onlyOnLanguage(this.languageKey);
  }

  @Override
  public void execute(SensorContext context) {
    if (!coverageAggregator.hasCoverageProperty()) {
      LOG.debug("No coverage property. Skip Sensor");
      return;
    }
    analyze(context, new Coverage());
  }

  void analyze(SensorContext context, Coverage coverage) {

    LOG.debug("Analyzing coverage with wildcardPatternFileProvider with base dir '{}' and file separator '{}'.",
      BASE_DIR.getAbsolutePath(), File.separator);

    coverageAggregator.aggregate(wildcardPatternFileProvider, coverage);

    Set<String> coverageFiles = coverage.files();
    FileCountStatistics fileCountStatistics = new FileCountStatistics(coverageFiles.size());

    LOG.debug("Analyzing coverage after aggregate found '{}' coverage files.", coverageFiles.size());

    for (String filePath : coverageFiles) {
      LOG.trace("Counting statistics for '{}'.", filePath);
      FilePredicates p = context.fileSystem().predicates();
      InputFile inputFile = context.fileSystem().inputFile(p.hasAbsolutePath(filePath));

      if (inputFile == null) {
        fileCountStatistics.projectExcluded++;
        LOG.debug("The file '{}' is either excluded or outside of your solution folder therefore Code "
          + "Coverage will not be imported.", filePath);
      } else if (inputFile.type().equals(Type.TEST)) {
        fileCountStatistics.test++;
        LOG.debug("Skipping '{}' as it is a test file.", filePath);
      } else if (!coverageConf.languageKey().equals(inputFile.language())) {
        LOG.debug("Skipping '{}' as conf lang '{}' does not equal file lang '{}'.", filePath, lazy(coverageConf::languageKey), lazy(inputFile::language));
        fileCountStatistics.otherLanguageExcluded++;
      } else {
        analyzeCoverage(context, coverage, fileCountStatistics, filePath, inputFile);
      }
    }

    LOG.debug("The total number of file count statistics is '{}'.", fileCountStatistics.total);

    if (fileCountStatistics.total != 0) {
      LOG.info("{}", lazy(fileCountStatistics::toString));
      if (fileCountStatistics.mainWithCoverage == 0) {
        LOG.warn("The Code Coverage report doesn't contain any coverage data for the included files. Troubleshooting guide: https://community.sonarsource.com/t/37151");
      }
    }
  }

  private static void analyzeCoverage(SensorContext context, Coverage coverage, FileCountStatistics fileCountStatistics, String filePath, InputFile inputFile) {
    LOG.trace("Checking main file coverage for '{}'.", filePath);
    fileCountStatistics.main++;
    boolean fileHasCoverage = false;

    NewCoverage newCoverage = context.newCoverage().onFile(inputFile);
    var coverageImportError = false;
    for (Map.Entry<Integer, Integer> entry : coverage.hits(filePath).entrySet()) {
      LOG.trace("Found entry with key '{}' and value '{}'.", entry.getKey(), entry.getValue());
      fileHasCoverage = true;
      var line = entry.getKey();
      if (line <= inputFile.lines()){
        newCoverage.lineHits(line, entry.getValue());
      } else {
        coverageImportError = true;
        LOG.debug("Coverage import: Line {} is out of range in the file '{}' (lines: {})", line, inputFile, inputFile.lines());
      }
    }
    if (coverageImportError){
      LOG.warn("Invalid data found in the coverage report, please check the debug logs for more details and raise an issue on the coverage tool being used.");
    }

    for (BranchCoverage branchCoverage : coverage.getBranchCoverage(filePath)) {
      LOG.trace("Found branch coverage entry on line '{}', with total conditions '{}' and covered conditions '{}'.",
        branchCoverage.getLine(), branchCoverage.getConditions(), branchCoverage.getCoveredConditions());
      fileHasCoverage = true;
      newCoverage.conditions(branchCoverage.getLine(), branchCoverage.getConditions(), branchCoverage.getCoveredConditions());
    }
    newCoverage.save();

    if (fileHasCoverage) {
      fileCountStatistics.mainWithCoverage++;
      LOG.trace("Found some coverage info for the file '{}'.", filePath);
    } else {
      LOG.debug("No coverage info found for the file '{}'.", filePath);
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
