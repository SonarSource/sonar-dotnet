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

import static org.hamcrest.Matchers.equalTo;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyDouble;
import static org.mockito.Matchers.anyString;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.Arrays;
import java.util.List;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;

public class CoverageDecoratorTest {

  private Project project;
  private VisualStudioProject vsProject;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;

  @Before
  public void init() {
    project = mock(Project.class);
    when(project.getLanguageKey()).thenReturn("cs");

    vsProject = mock(VisualStudioProject.class);

    microsoftWindowsEnvironment = mock(MicrosoftWindowsEnvironment.class);
    when(microsoftWindowsEnvironment.getCurrentProject(anyString())).thenReturn(vsProject);
  }

  @Test
  public void testDecorate() throws Exception {
    CoverageDecorator decorator = createDecorator();
    Resource<?> projectResource = new File("Foo");
    DecoratorContext context = mock(DecoratorContext.class);
    when(context.getMeasure(CoreMetrics.NCLOC)).thenReturn(new Measure(CoreMetrics.NCLOC, 100.0));
    when(context.getMeasure(CoreMetrics.STATEMENTS)).thenReturn(new Measure(CoreMetrics.STATEMENTS, 101.0));
    decorator.decorate(projectResource, context);
    verify(context, times(1)).saveMeasure(CoreMetrics.COVERAGE, 0.0);
    verify(context, times(1)).saveMeasure(CoreMetrics.LINE_COVERAGE, 0.0);
    verify(context, times(1)).saveMeasure(CoreMetrics.LINES_TO_COVER, 100.0);
    verify(context, times(1)).saveMeasure(CoreMetrics.UNCOVERED_LINES, 100.0);
  }

  // http://jira.codehaus.org/browse/SONARPLUGINS-1268
  @Test
  public void testDecorateWithNoNCLOC() throws Exception {
    CoverageDecorator decorator = createDecorator();
    Resource<?> projectResource = new File("Foo");
    DecoratorContext context = mock(DecoratorContext.class);
    when(context.getMeasure(CoreMetrics.NCLOC)).thenReturn(null);
    decorator.decorate(projectResource, context);
    verify(context, times(1)).saveMeasure(CoreMetrics.COVERAGE, 0.0);
    verify(context, times(1)).saveMeasure(CoreMetrics.LINE_COVERAGE, 0.0);
    verify(context, never()).saveMeasure(eq(CoreMetrics.LINES_TO_COVER), anyDouble());
    verify(context, never()).saveMeasure(eq(CoreMetrics.UNCOVERED_LINES), anyDouble());
  }

  @Test
  public void testDontDecorateIfNotFile() throws Exception {
    CoverageDecorator decorator = createDecorator();
    Resource<?> projectResource = new Project("Foo");
    DecoratorContext context = mock(DecoratorContext.class);
    decorator.decorate(projectResource, context);
    verify(context, never()).saveMeasure(any(Metric.class), anyDouble());
  }

  @Test
  public void testDontDecorateIfFileAlreadyHasCoverageMeasure() throws Exception {
    CoverageDecorator decorator = createDecorator();
    Resource<?> projectResource = new File("Foo");
    DecoratorContext context = mock(DecoratorContext.class);
    when(context.getMeasure(CoreMetrics.COVERAGE)).thenReturn(new Measure());
    decorator.decorate(projectResource, context);
    verify(context, never()).saveMeasure(any(Metric.class), anyDouble());
  }

  @Test
  public void testDependedUponMetrics() throws Exception {
    CoverageDecorator decorator = createDecorator();

    List<Metric> metrics = Arrays.asList(CoreMetrics.COVERAGE, CoreMetrics.LINE_COVERAGE, CoreMetrics.LINES_TO_COVER,
        CoreMetrics.UNCOVERED_LINES);

    assertThat(decorator.generatesCoverageMetrics(), equalTo(metrics));
  }

  @Test
  public void testShouldExecuteOnProject() throws Exception {
    CoverageDecorator decorator = createDecorator();
    assertTrue(decorator.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnNotCSharpProject() throws Exception {
    when(project.getLanguageKey()).thenReturn("java");
    microsoftWindowsEnvironment = mock(MicrosoftWindowsEnvironment.class);
    CoverageDecorator decorator = createDecorator();
    assertFalse(decorator.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnRootProject() throws Exception {
    when(project.isRoot()).thenReturn(true);
    CoverageDecorator decorator = createDecorator();
    assertFalse(decorator.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnProjectIfSkip() throws Exception {
    Configuration conf = new BaseConfiguration();
    conf.setProperty(GallioConstants.MODE, AbstractCSharpSensor.MODE_SKIP);
    CoverageDecorator decorator = new CoverageDecorator(new CSharpConfiguration(conf), microsoftWindowsEnvironment);
    assertFalse(decorator.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldNotExecuteOnTestProject() throws Exception {
    when(vsProject.isTest()).thenReturn(true);
    CoverageDecorator decorator = createDecorator();
    assertFalse(decorator.shouldExecuteOnProject(project));
  }

  private CoverageDecorator createDecorator() {
    Configuration conf = new BaseConfiguration();
    CoverageDecorator decorator = new CoverageDecorator(new CSharpConfiguration(conf), microsoftWindowsEnvironment);
    return decorator;
  }
}