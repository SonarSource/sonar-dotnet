/*
 * Sonar .NET Plugin :: Gallio
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
package org.sonar.plugins.csharp.gallio;

import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.utils.ResourceHelper;

import java.util.Arrays;
import java.util.List;

/**
 * Decorates resources that do not have coverage metrics because they were not touched by any test, and thus not present in the coverage
 * report file.
 */
public class ItCoverageDecorator extends CoverageDecorator {

  public ItCoverageDecorator(DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment,
      ResourceHelper resourceHelper) {

    super(configuration, microsoftWindowsEnvironment, resourceHelper);

    this.executionMode = configuration.getString(GallioConstants.IT_MODE_KEY);
    this.testMetric = CoreMetrics.IT_COVERAGE;
  }

  @Override
  protected void handleUncoveredResource(DecoratorContext context, double lines) {
    context.saveMeasure(CoreMetrics.IT_COVERAGE, 0.0);
    context.saveMeasure(CoreMetrics.IT_LINE_COVERAGE, 0.0);
    context.saveMeasure(CoreMetrics.IT_LINES_TO_COVER, lines);
    context.saveMeasure(CoreMetrics.IT_UNCOVERED_LINES, lines);
  }

  @DependedUpon
  public List<Metric> generatesCoverageMetrics() {
    return Arrays.asList(CoreMetrics.IT_COVERAGE, CoreMetrics.IT_LINE_COVERAGE, CoreMetrics.IT_LINES_TO_COVER, CoreMetrics.IT_UNCOVERED_LINES);
  }

}
