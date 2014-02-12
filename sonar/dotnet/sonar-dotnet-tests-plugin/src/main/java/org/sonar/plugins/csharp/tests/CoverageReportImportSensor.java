/*
 * Sonar .NET Plugin :: Tests
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.tests;

import com.google.common.annotations.VisibleForTesting;
import com.google.common.collect.ImmutableSet;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoverageMeasuresBuilder;
import org.sonar.api.measures.Measure;
import org.sonar.api.resources.Project;

import java.util.Map;
import java.util.Set;

public class CoverageReportImportSensor implements Sensor {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageReportImportSensor.class);
  private static final Set<String> SUPPORTED_LANGUAGES = ImmutableSet.of("cs", "vbnet");

  private final CoverageProviderFactory coverageProviderFactory;

  public CoverageReportImportSensor(CoverageProviderFactory coverageProviderFactory) {
    this.coverageProviderFactory = coverageProviderFactory;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return SUPPORTED_LANGUAGES.contains(project.getLanguageKey()) &&
      coverageProviderFactory.hasCoverageProperty();
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    analyze(context, new FileProvider(project));
  }

  @VisibleForTesting
  void analyze(SensorContext context, FileProvider fileProvider) {
    Coverage coverage = coverageProviderFactory.coverageProvider().coverage();
    CoverageMeasuresBuilder coverageMeasureBuilder = CoverageMeasuresBuilder.create();

    for (String filePath : coverage.files()) {
      org.sonar.api.resources.File file = fileProvider.fromPath(filePath);

      if (file != null) {
        coverageMeasureBuilder.reset();
        for (Map.Entry<Integer, Integer> entry : coverage.hits(filePath).entrySet()) {
          coverageMeasureBuilder.setHits(entry.getKey(), entry.getValue());
        }

        for (Measure measure : coverageMeasureBuilder.createMeasures()) {
          context.saveMeasure(file, measure);
        }
      } else {
        LOG.debug("Code coverage will not be imported for the following non-indexed file: " + filePath);
      }
    }
  }

}
