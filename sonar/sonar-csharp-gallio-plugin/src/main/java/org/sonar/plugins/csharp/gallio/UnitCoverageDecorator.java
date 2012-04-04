/*
 * Sonar C# Plugin :: Gallio
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

import java.util.Arrays;
import java.util.List;

import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.ResourceHelper;

public class UnitCoverageDecorator extends CoverageDecorator {

  public UnitCoverageDecorator(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment,
      ResourceHelper resourceHelper) {

    super(configuration, microsoftWindowsEnvironment, resourceHelper);

    this.executionMode = configuration.getString(GallioConstants.MODE, "");
    this.testMetric = CoreMetrics.COVERAGE;
  }

  @DependedUpon
  public List<Metric> generatesCoverageMetrics() {
    return Arrays.asList(CoreMetrics.COVERAGE, CoreMetrics.LINE_COVERAGE, CoreMetrics.LINES_TO_COVER, CoreMetrics.UNCOVERED_LINES);
  }

  @Override
  protected void handleUncoveredResource(DecoratorContext context, double lines) {
    context.saveMeasure(CoreMetrics.COVERAGE, 0.0);
    context.saveMeasure(CoreMetrics.LINE_COVERAGE, 0.0);
    context.saveMeasure(CoreMetrics.LINES_TO_COVER, lines);
    context.saveMeasure(CoreMetrics.UNCOVERED_LINES, lines);
  }
}
