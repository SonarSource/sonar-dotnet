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
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.resources.ResourceUtils;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.ResourceHelper;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;

import com.google.common.collect.Sets;

/**
 * Decorates resources that do not have coverage metrics because they were not touched by any test, and thus not present in the coverage
 * report file.
 */
public class CoverageDecorator implements Decorator {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageDecorator.class);

  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private String executionMode;
  private Set<String> excludedAssemblies;
  private VisualStudioSolution vsSolution;
  private ResourceHelper resourceHelper;

  public CoverageDecorator(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment, ResourceHelper resourceHelper) {
    this.microsoftWindowsEnvironment = microsoftWindowsEnvironment;
    this.vsSolution = microsoftWindowsEnvironment.getCurrentSolution();
    this.executionMode = configuration.getString(GallioConstants.MODE, "");
    String[] exclusions = configuration.getStringArray(GallioConstants.COVERAGE_EXCLUDES_KEY);
    if (exclusions==null) {
      this.excludedAssemblies 
        = Collections.EMPTY_SET;
    } else {
      this.excludedAssemblies 
        = Sets.newHashSet(exclusions);
    }
    this.resourceHelper = resourceHelper;
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (project.isRoot() || !CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey())) {
      return false;
    }
    boolean skipMode = AbstractCSharpSensor.MODE_SKIP.equalsIgnoreCase(executionMode);
    boolean isTestProject = microsoftWindowsEnvironment.getCurrentProject(project.getName()).isTest();
    return !isTestProject && !skipMode;
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
          context.saveMeasure(CoreMetrics.COVERAGE, 0.0);
          context.saveMeasure(CoreMetrics.LINE_COVERAGE, 0.0);
          context.saveMeasure(CoreMetrics.LINES_TO_COVER, lines);
          context.saveMeasure(CoreMetrics.UNCOVERED_LINES, lines);
        }
      }
    }
  }

  private boolean isExcludedFromCoverage(Resource resource) {
    if (excludedAssemblies.isEmpty()) {
      return false;
    }
    Project project = resourceHelper.findParentProject(resource);
    VisualStudioProject vsProject 
      = vsSolution.getProjectFromSonarProject(project);
    return excludedAssemblies.contains(vsProject.getName());
  }

}