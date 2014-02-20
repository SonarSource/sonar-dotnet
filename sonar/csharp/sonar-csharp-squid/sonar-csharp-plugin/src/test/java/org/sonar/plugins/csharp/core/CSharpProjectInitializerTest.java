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

package org.sonar.plugins.csharp.core;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.config.Settings;
import org.sonar.api.resources.Project;

import static org.fest.assertions.Assertions.assertThat;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CSharpProjectInitializerTest {

  private CSharpProjectInitializer initializer;
  private Project project;
  private Settings settings;

  @Before
  public void initProject() {
    settings = new Settings();
    project = mock(Project.class);
    Configuration deprecatedConf = new BaseConfiguration();
    // FIXME: deprecated
    when(project.getConfiguration()).thenReturn(deprecatedConf);

    initializer = new CSharpProjectInitializer(settings);
  }

  @Test
  public void shouldExecuteOnProject() {
    assertThat(initializer.shouldExecuteOnProject(project)).isFalse();

    // FIXME: deprecated
    when(project.getLanguageKey()).thenReturn("cs");
    assertThat(initializer.shouldExecuteOnProject(project)).isTrue();
  }

  @Test
  public void shouldNotSetDefaultExclusions() throws Exception {
    settings.setProperty("sonar.dotnet.excludeGeneratedCode", false);
    initializer.execute(project);
    assertThat(settings.getStringArray("sonar.exclusions"), is(new String[0]));
  }

  @Test
  public void shouldAddExclusions() throws Exception {
    settings.setProperty("sonar.exclusions", "Foo.cs,**/Bar.cs");
    initializer.execute(project);
    String[] exclusions = settings.getStringArray("sonar.exclusions");
    assertThat(exclusions[0], is("Foo.cs"));
    assertThat(exclusions[1], is("**/Bar.cs"));
  }

}
