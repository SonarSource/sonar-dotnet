/*
 * Sonar C# Plugin :: Core
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

package org.sonar.plugins.csharp.api;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.nullValue;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;

public class ResourceHelperTest {

  @Test
  public void testIsResourceInProjectForProject() throws Exception {
    ResourceHelper helper = new ResourceHelper(null);
    Resource<?> resource = new Project("foo");
    assertThat(helper.isResourceInProject(resource, new Project("foo")), is(true));
    assertThat(helper.isResourceInProject(resource, new Project("bar")), is(false));
  }

  @Test
  public void testIsResourceInProjectForResourceWithNoParent() throws Exception {
    SensorContext context = mock(SensorContext.class);
    Resource<?> resource = new File("foo");
    Project project = new Project("bar");
    ResourceHelper helper = new ResourceHelper(context);
    assertThat(helper.isResourceInProject(resource, project), is(false));
  }

  @Test
  public void testIsResourceInProjectForResourceWithParent() throws Exception {
    SensorContext context = mock(SensorContext.class);
    Resource<?> resource = new File("foo");
    Project project = new Project("bar");
    when(context.getParent(resource)).thenReturn(project);
    ResourceHelper helper = new ResourceHelper(context);
    assertThat(helper.isResourceInProject(resource, new Project("bar")), is(true));
    assertThat(helper.isResourceInProject(resource, new Project("bar2")), is(false));
  }

  @Test
  public void testfindParentProjectForProject() throws Exception {
    ResourceHelper helper = new ResourceHelper(null);
    Project resource = new Project("foo");
    assertThat(helper.findParentProject(resource), is(resource));
  }

  @Test
  public void testfindParentProjectForProjectWithNoParent() throws Exception {
    SensorContext context = mock(SensorContext.class);
    ResourceHelper helper = new ResourceHelper(context);
    Resource<?> resource = new File("foo");
    assertThat(helper.findParentProject(resource), is(nullValue()));
  }

  @Test
  public void testfindParentProjectForProjectWithParent() throws Exception {
    SensorContext context = mock(SensorContext.class);
    ResourceHelper helper = new ResourceHelper(context);
    Resource<?> resource = new File("foo");
    Project project = new Project("bar");
    when(context.getParent(resource)).thenReturn(project);
    assertThat(helper.findParentProject(resource), is(project));
  }

}
