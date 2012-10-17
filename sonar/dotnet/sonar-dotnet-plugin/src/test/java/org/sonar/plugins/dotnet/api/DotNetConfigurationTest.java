/*
 * Sonar .NET Plugin :: Core
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
package org.sonar.plugins.dotnet.api;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.config.Settings;
import org.sonar.plugins.dotnet.core.DotNetCorePlugin;

import static org.fest.assertions.Assertions.assertThat;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;

public class DotNetConfigurationTest {

  private Settings settings;
  private DotNetConfiguration dotNetConfiguration;

  @Before
  public void init() {
    settings = Settings.createForComponent(new DotNetCorePlugin());
    dotNetConfiguration = new DotNetConfiguration(settings);
  }

  @Test
  public void testConfigHasOldParameters() throws Exception {
    // old params
    settings.setProperty("visual.studio.solution", "foo.sln");
    settings.setProperty("visual.test.project.pattern", "foo*Test");
    settings.setProperty("dotnet.tool.version", "bar1");
    settings.setProperty("silverlight.version", "bar2");

    // new params
    settings.setProperty(DotNetConstants.SOLUTION_FILE_KEY, "NEW-foo.sln");
    settings.setProperty(DotNetConstants.TEST_PROJECT_PATTERN_KEY, "NEW-foo*Test");
    settings.setProperty(DotNetConstants.DOTNET_VERSION_KEY, "NEW-bar1");
    settings.setProperty(DotNetConstants.SILVERLIGHT_VERSION_KEY, "NEW-bar2");

    assertThat(dotNetConfiguration.getString(DotNetConstants.SOLUTION_FILE_KEY), is("foo.sln"));
    assertThat(dotNetConfiguration.getString(DotNetConstants.TEST_PROJECT_PATTERN_KEY), is("foo*Test"));
    assertThat(dotNetConfiguration.getString(DotNetConstants.DOTNET_VERSION_KEY), is("bar1"));
    assertThat(dotNetConfiguration.getString(DotNetConstants.SILVERLIGHT_VERSION_KEY), is("bar2"));
  }

  @Test
  public void testConfigHasSomeOldParameters() throws Exception {
    // old params
    settings.setProperty("visual.studio.solution", "foo.sln");
    settings.setProperty("visual.test.project.pattern", "foo*Test");
    settings.setProperty("dotnet.tool.version", "");

    // new params
    settings.setProperty(DotNetConstants.SOLUTION_FILE_KEY, "NEW-foo.sln");
    settings.setProperty(DotNetConstants.TEST_PROJECT_PATTERN_KEY, "NEW-foo*Test");
    settings.setProperty(DotNetConstants.DOTNET_VERSION_KEY, "NEW-bar1");
    settings.setProperty(DotNetConstants.SILVERLIGHT_VERSION_KEY, "NEW-bar2");
    settings.setProperty("some.new.param", "NEW-param");

    assertThat(dotNetConfiguration.getString(DotNetConstants.SOLUTION_FILE_KEY), is("foo.sln"));
    assertThat(dotNetConfiguration.getString(DotNetConstants.TEST_PROJECT_PATTERN_KEY), is("foo*Test"));
    assertThat(dotNetConfiguration.getString(DotNetConstants.DOTNET_VERSION_KEY), is("NEW-bar1"));
    assertThat(dotNetConfiguration.getString(DotNetConstants.SILVERLIGHT_VERSION_KEY), is("NEW-bar2"));
    assertThat(dotNetConfiguration.getString("some.new.param"), is("NEW-param"));
  }

  @Test
  public void testDifferentParameterTypes() throws Exception {
    // old params
    settings.setProperty("fxcop.ignore.generated.code", "true");
    settings.setProperty("fxcop.additionalDirectories", "foo;bar");

    // new params
    settings.setProperty("sonar.fxcop.ignoreGeneratedCode", "false");
    settings.setProperty("sonar.fxcop.assemblyDependencyDirectories", "toto,tutu");

    assertTrue(dotNetConfiguration.getBoolean("sonar.fxcop.ignoreGeneratedCode"));
    assertThat(dotNetConfiguration.getStringArray("sonar.fxcop.assemblyDependencyDirectories")[0], is("foo"));
    assertThat(dotNetConfiguration.getStringArray("sonar.fxcop.assemblyDependencyDirectories")[1], is("bar"));
  }

  @Test
  public void testSetProperty() {
    dotNetConfiguration.setProperty("foo", "bar");
    assertThat(dotNetConfiguration.getString("foo")).isEqualTo("bar");

    dotNetConfiguration.setProperty("foo", new String[] {"bar1", "bar2", "bar3"});
    assertThat(dotNetConfiguration.getString("foo")).isEqualTo("bar1,bar2,bar3");
  }

}
