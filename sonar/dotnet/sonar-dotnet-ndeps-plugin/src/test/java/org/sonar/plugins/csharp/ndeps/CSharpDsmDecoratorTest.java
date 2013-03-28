/*
 * Sonar .NET Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Matchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

/*
 * Not too much test effort here as CSharpDsmDecorator will be removed once the design part of Sonar will be available in the API.
 */
public class CSharpDsmDecoratorTest {

  private CSharpDsmDecorator dsmDecorator;
  private DecoratorContext context;
  private Resource<?> resource;
  private VisualStudioProject vsProject;

  @Before
  public void init() {
    MicrosoftWindowsEnvironment microsoftWindowsEnvironment = mock(MicrosoftWindowsEnvironment.class);
    vsProject = mock(VisualStudioProject.class);
    when(microsoftWindowsEnvironment.getCurrentProject("Foo")).thenReturn(vsProject);

    dsmDecorator = new CSharpDsmDecorator(null, microsoftWindowsEnvironment);
    resource = mock(Resource.class);
    when(resource.getScope()).thenReturn("DIR");
    context = mock(DecoratorContext.class);
    when(context.getResource()).thenReturn(resource);
  }

  @Test
  public void testShouldNotExecuteOnNoCSharpProject() throws Exception {
    Project p = new Project("");
    assertThat(dsmDecorator.shouldExecuteOnProject(p), is(false));
  }

  @Test
  public void testShouldNotExecuteOnRootProject() throws Exception {
    Project p = new Project("");
    p.setLanguageKey("cs");
    assertThat(dsmDecorator.shouldExecuteOnProject(p), is(false));
  }

  @Test
  public void testShouldExecute() throws Exception {
    Project p = new Project("");
    p.setLanguageKey("cs");
    p.setName("Foo");
    p.setParent(p);
    assertThat(dsmDecorator.shouldExecuteOnProject(p), is(true));
  }

  @Test
  public void testShouldNotExecuteOnWebProject() throws Exception {
    Project p = new Project("");
    p.setLanguageKey("cs");
    p.setName("Foo");
    p.setParent(p);
    when(vsProject.isWebProject()).thenReturn(true);
    assertThat(dsmDecorator.shouldExecuteOnProject(p), is(false));
  }

  @Test
  public void shouldNotBeExecutedIfDsmAlreadyComputed() {
    // given that
    when(context.getMeasure(CoreMetrics.DEPENDENCY_MATRIX)).thenReturn(new Measure(CoreMetrics.DEPENDENCY_MATRIX, "..."));

    // when
    dsmDecorator.decorate(resource, context);

    // then
    verify(context, never()).saveMeasure(any(Measure.class));
  }

  @Test
  public void shouldNotBeExecutedIfFile() {
    // given that
    when(resource.getScope()).thenReturn("FIL");

    // when
    dsmDecorator.decorate(resource, context);

    // then
    verify(context, never()).saveMeasure(any(Measure.class));
  }

  @Test
  public void shouldExecuteIfDirAndDsmNotComputed() throws Exception {
    // when
    dsmDecorator.decorate(resource, context);

    // then
    verify(context, times(1)).saveMeasure(any(Measure.class));
  }

  @Test
  public void shouldExecuteIfModuleAndDsmNotComputed() throws Exception {
    // given that
    when(resource.getQualifier()).thenReturn("BRC");

    // when
    dsmDecorator.decorate(resource, context);

    // then
    verify(context, times(1)).saveMeasure(any(Measure.class));
  }

}
