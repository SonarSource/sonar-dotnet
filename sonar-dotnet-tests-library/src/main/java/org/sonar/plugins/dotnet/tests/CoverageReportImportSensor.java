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

import com.google.common.annotations.VisibleForTesting;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.batch.sensor.coverage.CoverageType;
import org.sonar.api.batch.sensor.coverage.NewCoverage;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.File;
import java.util.Map;

public class CoverageReportImportSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CoverageReportImportSensor.class);

  private final WildcardPatternFileProvider wildcardPatternFileProvider = new WildcardPatternFileProvider(new File("."), File.separator);
  private final CoverageConfiguration coverageConf;
  private final CoverageAggregator coverageAggregator;
  private final boolean isIntegrationTest;
  private final String languageKey;
  private final String languageName;

  public CoverageReportImportSensor(CoverageConfiguration coverageConf, CoverageAggregator coverageAggregator, String languageKey, String languageName, boolean isIntegrationTest) {
    this.coverageConf = coverageConf;
    this.coverageAggregator = coverageAggregator;
    this.isIntegrationTest = isIntegrationTest;
    this.languageKey = languageKey;
    this.languageName = languageName;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    if (this.isIntegrationTest) {
      descriptor.name(this.languageName + " Integration Tests Coverage Report Import");
    } else {
      descriptor.name(this.languageName + " Unit Tests Coverage Report Import");
    }
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

  @VisibleForTesting
  void analyze(SensorContext context, Coverage coverage) {
    coverageAggregator.aggregate(wildcardPatternFileProvider, coverage);

    for (String filePath : coverage.files()) {
      FilePredicates p = context.fileSystem().predicates();
      InputFile inputFile = context.fileSystem().inputFile(p.and(p.hasType(Type.MAIN), p.hasAbsolutePath(filePath)));

      if (inputFile == null) {
        LOG.debug("Code coverage will not be imported for the following file outside of SonarQube: " + filePath);
        continue;
      }

      if (!coverageConf.languageKey().equals(inputFile.language())) {
        continue;
      }

      NewCoverage newCoverage = context.newCoverage()
        .onFile(inputFile)
        .ofType(isIntegrationTest ? CoverageType.IT : CoverageType.UNIT);

      for (Map.Entry<Integer, Integer> entry : coverage.hits(filePath).entrySet()) {
        newCoverage.lineHits(entry.getKey(), entry.getValue());
      }

      newCoverage.save();
    }
  }

}
