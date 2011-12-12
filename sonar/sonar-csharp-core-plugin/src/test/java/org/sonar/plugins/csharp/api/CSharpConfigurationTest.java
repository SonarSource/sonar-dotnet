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
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.junit.Before;
import org.junit.Test;

public class CSharpConfigurationTest {

  private CSharpConfiguration cSharpConfiguration;
  private Configuration configuration;

  @Before
  public void init() {
    configuration = new BaseConfiguration();
    cSharpConfiguration = new CSharpConfiguration(configuration);
  }

  @Test
  public void testConfigHasOldParameters() throws Exception {
    // old params
    configuration.addProperty("visual.studio.solution", "foo.sln");
    configuration.addProperty("visual.test.project.pattern", "foo*Test");
    configuration.addProperty("dotnet.tool.version", "bar1");
    configuration.addProperty("silverlight.version", "bar2");

    // new params
    configuration.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "NEW-foo.sln");
    configuration.addProperty(CSharpConstants.TEST_PROJECT_PATTERN_KEY, "NEW-foo*Test");
    configuration.addProperty(CSharpConstants.DOTNET_VERSION_KEY, "NEW-bar1");
    configuration.addProperty(CSharpConstants.SILVERLIGHT_VERSION_KEY, "NEW-bar2");

    assertThat(cSharpConfiguration.getString(CSharpConstants.SOLUTION_FILE_KEY, CSharpConstants.SOLUTION_FILE_DEFVALUE), is("foo.sln"));
    assertThat(cSharpConfiguration.getString(CSharpConstants.TEST_PROJECT_PATTERN_KEY, CSharpConstants.TEST_PROJECT_PATTERN_DEFVALUE),
        is("foo*Test"));
    assertThat(cSharpConfiguration.getString(CSharpConstants.DOTNET_VERSION_KEY, CSharpConstants.DOTNET_VERSION_DEFVALUE), is("bar1"));
    assertThat(cSharpConfiguration.getString(CSharpConstants.SILVERLIGHT_VERSION_KEY, CSharpConstants.SILVERLIGHT_VERSION_DEFVALUE),
        is("bar2"));
  }

  @Test
  public void testConfigHasSomeOldParameters() throws Exception {
    // old params
    configuration.addProperty("visual.studio.solution", "foo.sln");
    configuration.addProperty("visual.test.project.pattern", "foo*Test");
    configuration.addProperty("dotnet.tool.version", "");

    // new params
    configuration.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "NEW-foo.sln");
    configuration.addProperty(CSharpConstants.TEST_PROJECT_PATTERN_KEY, "NEW-foo*Test");
    configuration.addProperty(CSharpConstants.DOTNET_VERSION_KEY, "NEW-bar1");
    configuration.addProperty(CSharpConstants.SILVERLIGHT_VERSION_KEY, "NEW-bar2");
    configuration.addProperty("some.new.param", "NEW-param");

    assertThat(cSharpConfiguration.getString(CSharpConstants.SOLUTION_FILE_KEY, CSharpConstants.SOLUTION_FILE_DEFVALUE), is("foo.sln"));
    assertThat(cSharpConfiguration.getString(CSharpConstants.TEST_PROJECT_PATTERN_KEY, CSharpConstants.TEST_PROJECT_PATTERN_DEFVALUE),
        is("foo*Test"));
    assertThat(cSharpConfiguration.getString(CSharpConstants.DOTNET_VERSION_KEY, CSharpConstants.DOTNET_VERSION_DEFVALUE), is("NEW-bar1"));
    assertThat(cSharpConfiguration.getString(CSharpConstants.SILVERLIGHT_VERSION_KEY, CSharpConstants.SILVERLIGHT_VERSION_DEFVALUE),
        is("NEW-bar2"));
    assertThat(cSharpConfiguration.getString("some.new.param", "default"), is("NEW-param"));
  }

  @Test
  public void testDifferentParameterTypes() throws Exception {
    // old params
    configuration.addProperty("fxcop.ignore.generated.code", "true");
    configuration.addProperty("fxcop.additionalDirectories", "foo;bar");

    // new params
    configuration.addProperty("sonar.fxcop.ignoreGeneratedCode", "false");
    configuration.addProperty("sonar.fxcop.assemblyDependencyDirectories", "toto,tutu");

    assertTrue(cSharpConfiguration.getBoolean("sonar.fxcop.ignoreGeneratedCode", false));
    assertThat(cSharpConfiguration.getStringArray("sonar.fxcop.assemblyDependencyDirectories")[0], is("foo"));
    assertThat(cSharpConfiguration.getStringArray("sonar.fxcop.assemblyDependencyDirectories")[1], is("bar"));
  }

}
