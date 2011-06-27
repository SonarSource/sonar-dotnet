/*
 * .NET tools :: Gallio Runner
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
package org.sonar.dotnet.tools.gallio;

import static org.hamcrest.Matchers.endsWith;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.utils.command.Command;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class GallioCommandBuilderTest {

  private static VisualStudioSolution solution;

  @BeforeClass
  public static void initData() {
    VisualStudioProject vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getArtifact("Debug")).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Fake1.assembly"));
    VisualStudioProject vsProject2 = mock(VisualStudioProject.class);
    when(vsProject2.getArtifact("Debug")).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Fake2.assembly"));
    solution = mock(VisualStudioSolution.class);
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsProject1, vsProject2));
  }

  @Test
  public void testToCommandForSolution() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/FakeProg"));
    builder.setReportFile(new File("target/sonar/gallio-report-folder/gallio-report.xml"));
    Command command = builder.toCommand();

    assertThat(command.getExecutable(), endsWith("Gallio.Echo.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands.length, is(6));
    assertThat(commands[0], is("/r:IsolatedProcess"));
    assertThat(commands[1], endsWith("gallio-report-folder"));
    assertThat(commands[2], is("/report-name-format:gallio-report"));
    assertThat(commands[3], is("/report-type:Xml"));
    assertThat(commands[4], endsWith("Fake1.assembly"));
    assertThat(commands[5], endsWith("Fake2.assembly"));
  }

  @Test
  public void testToCommandForSolutionWithMoreParams() throws Exception {
    VisualStudioProject vsProject = mock(VisualStudioProject.class);
    when(vsProject.getArtifact("Release")).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Fake2.assembly"));
    solution = mock(VisualStudioSolution.class);
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsProject));

    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/FakeProg"));
    builder.setReportFile(new File("target/sonar/gallio-report-folder/gallio-report.xml"));
    builder.setFilter("FooFilter");
    builder.setBuildConfigurations("Release");
    Command command = builder.toCommand();

    assertThat(command.getExecutable(), endsWith("Gallio.Echo.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands.length, is(6));
    assertThat(commands[0], is("/r:IsolatedProcess"));
    assertThat(commands[1], endsWith("gallio-report-folder"));
    assertThat(commands[2], is("/report-name-format:gallio-report"));
    assertThat(commands[3], is("/report-type:Xml"));
    assertThat(commands[4], endsWith("/f:FooFilter"));
    assertThat(commands[5], endsWith("Fake2.assembly"));
  }

  @Test(expected = GallioException.class)
  public void testNoConfigFile() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/FakeProg"));
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testNoSolutionOrProject() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder((VisualStudioSolution) null);
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testUnexistingInstallDir() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/UnexistingProgDir"));
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testInstallDirNotDir() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/FakeProg/Gallio.Echo.exe"));
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testWrongInstallDir() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/FakeAssemblies"));
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testNoTestAssembly() throws Exception {
    VisualStudioProject vsProject = mock(VisualStudioProject.class);
    when(vsProject.getArtifact("Debug")).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Unexisting.assembly"));
    solution = mock(VisualStudioSolution.class);
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsProject));
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setInstallationFolder(TestUtils.getResource("/Runner/FakeProg"));
    builder.setReportFile(new File("target/sonar/gallio-report-folder/gallio-report.xml"));
    builder.toCommand();
  }

}
