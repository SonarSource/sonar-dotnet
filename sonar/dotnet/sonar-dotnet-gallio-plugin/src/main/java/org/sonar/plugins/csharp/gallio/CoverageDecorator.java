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

import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;

import com.google.common.collect.Sets;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.resources.ResourceUtils;
import org.sonar.dotnet.tools.gallio.GallioRunnerConstants;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.sensor.AbstractDotNetSensor;
import org.sonar.plugins.dotnet.api.utils.ResourceHelper;

import java.util.Collections;
import java.util.Set;

/**
 * Decorates resources that do not have coverage metrics because they were not touched by any test, and thus not present in the coverage
 * report file.
 */
public abstract class CoverageDecorator implements Decorator {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageDecorator.class);

  protected MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  protected String executionMode;
  protected Set<String> excludedAssemblies;
  protected VisualStudioSolution vsSolution;
  protected ResourceHelper resourceHelper;
  protected Metric testMetric;
  protected DotNetConfiguration configuration;

  protected CoverageDecorator(DotNetConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment, ResourceHelper resourceHelper) {
    this.microsoftWindowsEnvironment = microsoftWindowsEnvironment;
    this.vsSolution = microsoftWindowsEnvironment.getCurrentSolution();
    String[] exclusions = configuration.getStringArray(GallioConstants.COVERAGE_EXCLUDES_KEY);
    if (exclusions == null) {
      this.excludedAssemblies = Collections.EMPTY_SET;
    } else {
      this.excludedAssemblies = Sets.newHashSet(exclusions);
    }
    this.resourceHelper = resourceHelper;
    this.configuration = configuration;
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (project.isRoot() || !GallioConstants.isLanguageSupported(project.getLanguageKey())) {
      return false;
    }
    boolean skipMode = AbstractDotNetSensor.MODE_SKIP.equalsIgnoreCase(executionMode);
    boolean isTestProject = microsoftWindowsEnvironment.getCurrentProject(project.getName()).isTest();
    boolean coverageToolIsNone = GallioRunnerConstants.COVERAGE_TOOL_NONE_KEY.equals(
        configuration.getString(GallioConstants.COVERAGE_TOOL_KEY));
    return !isTestProject && !skipMode && !coverageToolIsNone;
  }

  /**
   * {@inheritDoc}
   */
  @SuppressWarnings("rawtypes")
  public void decorate(Resource resource, DecoratorContext context) {
    if (ResourceUtils.isFile(resource) && context.getMeasure(testMetric) == null) {
      if (isExcludedFromCoverage(resource)) {
        return;
      }
      // for LINES_TO_COVER and UNCOVERED_LINES, we use NCLOC and STATEMENTS as an approximation
      Measure ncloc = context.getMeasure(CoreMetrics.NCLOC);
      if (ncloc != null) {
        Measure sts = context.getMeasure(CoreMetrics.STATEMENTS);
        double lines = Math.min(ncloc.getValue(), sts.getValue());
        if (lines > 0d) {
          LOG.debug("Coverage metrics have not been set on '{}': default values will be inserted.", resource.getName());
          handleUncoveredResource(context, lines);
        }
      }
    }
  }

  protected abstract void handleUncoveredResource(DecoratorContext context, double lines);

  private boolean isExcludedFromCoverage(Resource resource) {
    if (excludedAssemblies.isEmpty()) {
      return false;
    }
    Project project = resourceHelper.findParentProject(resource);
    VisualStudioProject vsProject = vsSolution.getProjectFromSonarProject(project);
    return excludedAssemblies.contains(vsProject.getName());
  }

}
