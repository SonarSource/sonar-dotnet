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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.resources.ResourceUtils;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

/**
 * Decorates resources that do not have coverage metrics because they were not touched by any test, and thus not present in the coverage
 * report file.
 */
public class CoverageDecorator implements Decorator {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageDecorator.class);

  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private String executionMode;

  public CoverageDecorator(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    this.microsoftWindowsEnvironment = microsoftWindowsEnvironment;
    this.executionMode = configuration.getString(GallioConstants.MODE, "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (project.isRoot()) {
      return false;
    }
    boolean skipMode = GallioConstants.MODE_SKIP.equalsIgnoreCase(executionMode);
    boolean isTestProject = microsoftWindowsEnvironment.getCurrentProject(project.getName()).isTest();
    return CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey()) && !isTestProject && !skipMode;
  }

  @DependedUpon
  public List<Metric> generatesCoverageMetrics() {
    return Arrays.asList(CoreMetrics.COVERAGE, CoreMetrics.LINE_COVERAGE, CoreMetrics.LINES_TO_COVER, CoreMetrics.UNCOVERED_LINES);
  }

  /**
   * {@inheritDoc}
   */
  @SuppressWarnings("rawtypes")
  public void decorate(Resource resource, DecoratorContext context) {
    if (ResourceUtils.isFile(resource) && context.getMeasure(CoreMetrics.COVERAGE) == null) {
      LOG.info("Coverage metrics have not been set on '{}': default values will be inserted.", resource.getName());
      context.saveMeasure(CoreMetrics.COVERAGE, 0.0);
      context.saveMeasure(CoreMetrics.LINE_COVERAGE, 0.0);
      // for LINES_TO_COVER and UNCOVERED_LINES, we use NCLOC as an approximation
      double ncloc = context.getMeasure(CoreMetrics.NCLOC).getValue();
      context.saveMeasure(CoreMetrics.LINES_TO_COVER, ncloc);
      context.saveMeasure(CoreMetrics.UNCOVERED_LINES, ncloc);
    }
  }

}