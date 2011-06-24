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

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.notNullValue;
import static org.hamcrest.Matchers.nullValue;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.util.Properties;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

public class VisualStudioProjectBuilderTest {

  private static File fakeSdkDir;
  private static File fakeSilverlightDir;
  private static MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private ProjectReactor reactor;
  private ProjectDefinition root;
  private File solutionBaseDir;
  private VisualStudioProjectBuilder projectBuilder;
  private Configuration conf;

  @BeforeClass
  public static void initResources() {
    fakeSdkDir = new File("target/sonar/SDK");
    fakeSdkDir.mkdirs();
    fakeSilverlightDir = new File("target/sonar/Silverlight");
    fakeSilverlightDir.mkdirs();
    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
  }

  @AfterClass
  public static void removeResources() {
    fakeSdkDir.delete();
    fakeSilverlightDir.delete();
  }

  @Before
  public void initBuilder() {
    conf = new BaseConfiguration();
    conf.addProperty("sonar.language", "cs");
    conf.addProperty(CSharpConstants.DOTNET_4_0_SDK_DIR_KEY, fakeSdkDir.getAbsolutePath());
    conf.addProperty(CSharpConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_KEY, fakeSilverlightDir.getAbsolutePath());
    solutionBaseDir = TestUtils.getResource("/solution/Example");
    root = ProjectDefinition.create(new Properties()).setBaseDir(solutionBaseDir).setWorkDir(new File(solutionBaseDir, "WORK-DIR"));
    root.setVersion("1.0");
    root.setKey("groupId:artifactId");
    reactor = new ProjectReactor(root);
    projectBuilder = new VisualStudioProjectBuilder(reactor, new CSharpConfiguration(conf), microsoftWindowsEnvironment);
  }

  @Test
  public void testEnhanceRootProperties() throws Exception {
    root.getProperties().put("fake", "foo");
    assertThat(root.getProperties().getProperty("sonar.sourceEncoding"), nullValue());
    Properties props = projectBuilder.enhanceRootProperties(root);
    assertThat(props.getProperty("fake"), is("foo"));
    assertThat(props.getProperty("sonar.sourceEncoding"), is("UTF-8"));
    assertThat(props.getProperty("sonar.exclusions"), is("**/Reference.cs,**/*.designer.cs"));
  }

  @Test
  public void testEnhanceRootPropertiesWithDefinedSonarExclusions() throws Exception {
    root.getProperties().put("sonar.exclusions", "**/Foo.cs,Toto.cs");
    Properties props = projectBuilder.enhanceRootProperties(root);
    assertThat(props.getProperty("sonar.exclusions"), is("**/Foo.cs,Toto.cs,**/Reference.cs,**/*.designer.cs"));
  }

  @Test
  public void testEnhanceRootPropertiesWithGeneratedCodeNotExcluded() throws Exception {
    conf.addProperty("sonar.dotnet.excludeGeneratedCode", Boolean.FALSE);
    projectBuilder = new VisualStudioProjectBuilder(reactor, new CSharpConfiguration(conf), microsoftWindowsEnvironment);
    root.getProperties().put("sonar.exclusions", "**/Foo.cs,Toto.cs");
    Properties props = projectBuilder.enhanceRootProperties(root);
    assertThat(props.getProperty("sonar.exclusions"), is("**/Foo.cs,Toto.cs"));
  }

  @Test(expected = SonarException.class)
  public void testNotValidSdkDir() throws Exception {
    conf = new BaseConfiguration();
    conf.addProperty("sonar.language", "cs");
    conf.addProperty(CSharpConstants.DOTNET_4_0_SDK_DIR_KEY, "foo");
    projectBuilder = new VisualStudioProjectBuilder(reactor, new CSharpConfiguration(conf), microsoftWindowsEnvironment);
    projectBuilder.build(reactor);
  }

  @Test(expected = SonarException.class)
  public void testNotValidSilverlightDir() throws Exception {
    conf = new BaseConfiguration();
    conf.addProperty("sonar.language", "cs");
    conf.addProperty(CSharpConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_KEY, "foo");
    projectBuilder = new VisualStudioProjectBuilder(reactor, new CSharpConfiguration(conf), microsoftWindowsEnvironment);
    projectBuilder.build(reactor);
  }

  @Test(expected = SonarException.class)
  public void testNonExistingSlnFile() throws Exception {
    conf.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "NonExistingFile.sln");
    projectBuilder.build(reactor);
  }

  @Test
  public void testCorrectlyConfiguredProject() throws Exception {
    conf.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "Example.sln");
    projectBuilder.build(reactor);
    // check that the configuration is OK
    assertThat(microsoftWindowsEnvironment.getDotnetVersion(), is("4.0"));
    assertThat(microsoftWindowsEnvironment.getDotnetSdkDirectory().getAbsolutePath(), is(fakeSdkDir.getAbsolutePath()));
    assertThat(microsoftWindowsEnvironment.getSilverlightVersion(), is("4"));
    assertThat(microsoftWindowsEnvironment.getSilverlightDirectory().getAbsolutePath(), is(fakeSilverlightDir.getAbsolutePath()));
    // check that the solution is built
    VisualStudioSolution solution = microsoftWindowsEnvironment.getCurrentSolution();
    assertNotNull(solution);
    assertThat(solution.getProjects().size(), is(3));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Example.Application").getSourceFiles().size(), is(2));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Example.Core").getSourceFiles().size(), is(6));
    // check the multi-module definition is correct
    assertThat(reactor.getRoot().getSubProjects().size(), is(3));
    assertThat(reactor.getRoot().getSourceFiles().size(), is(0));
    ProjectDefinition subProject = reactor.getRoot().getSubProjects().get(0);
    VisualStudioProject vsProject = microsoftWindowsEnvironment.getCurrentProject("Example.Application");
    assertThat(subProject.getName(), is("Example.Application"));
    assertThat(subProject.getKey(), is("groupId:Example.Application"));
    assertThat(subProject.getVersion(), is("1.0"));
    assertThat(subProject.getBaseDir(), is(vsProject.getDirectory()));
    assertThat(subProject.getWorkDir(), is(new File(vsProject.getDirectory(), "WORK-DIR")));
    assertThat(subProject.getSourceDirs().iterator().next(), notNullValue());
    assertTrue(subProject.getTestDirs().isEmpty());
    ProjectDefinition testSubProject = reactor.getRoot().getSubProjects().get(2);
    assertThat(testSubProject.getName(), is("Example.Core.Tests"));
    assertThat(testSubProject.getTestDirs().iterator().next(), notNullValue());
    assertTrue(testSubProject.getSourceDirs().isEmpty());
  }

  @Test
  public void testNoSpecifiedSlnFileButOneFound() throws Exception {
    conf.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "");
    projectBuilder = new VisualStudioProjectBuilder(reactor, new CSharpConfiguration(conf), new MicrosoftWindowsEnvironment());
    projectBuilder.build(reactor);
    assertThat(microsoftWindowsEnvironment.getDotnetSdkDirectory().getAbsolutePath(), is(fakeSdkDir.getAbsolutePath()));
    VisualStudioSolution solution = microsoftWindowsEnvironment.getCurrentSolution();
    assertNotNull(solution);
    assertThat(solution.getProjects().size(), is(3));
  }

  @Test(expected = SonarException.class)
  public void testNoSpecifiedSlnFileButNoneFound() throws Exception {
    conf.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "");
    root.setBaseDir(TestUtils.getResource("/solution"));
    projectBuilder.build(reactor);
  }

  @Test(expected = SonarException.class)
  public void testNoSpecifiedSlnFileButTooManyFound() throws Exception {
    conf.addProperty(CSharpConstants.SOLUTION_FILE_KEY, "");
    root.setBaseDir(TestUtils.getResource("/solution/FakeSolutionWithTwoSlnFiles"));
    projectBuilder.build(reactor);
  }

}
