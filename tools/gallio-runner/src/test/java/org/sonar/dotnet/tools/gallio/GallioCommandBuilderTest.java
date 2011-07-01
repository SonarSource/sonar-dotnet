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

import static org.hamcrest.Matchers.containsString;
import static org.hamcrest.Matchers.endsWith;
import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.startsWith;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.utils.command.Command;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class GallioCommandBuilderTest {

  private static final File GALLIO_EXE = TestUtils.getResource("/Runner/FakeProg/Gallio/bin/Gallio.Echo.exe");
  private static final File GALLIO_REPORT_FILE = new File("target/sonar/gallio-report-folder/gallio-report.xml");
  private static final File PART_COVER_INSTALL_DIR = TestUtils.getResource("/Runner/FakeProg/PartCover");
  private static final File GALLIO_COVERAGE_REPORT_FILE = new File("target/sonar/gallio-report-folder/coverage-report.xml");
  private static final File FAKE_ASSEMBLY_2 = TestUtils.getResource("/Runner/FakeAssemblies/Fake2.assembly");
  private static final File FAKE_ASSEMBLY_1 = TestUtils.getResource("/Runner/FakeAssemblies/Fake1.assembly");
  private static final File WORK_DIR = TestUtils.getResource("/Runner/FakeWorkDir");
  private VisualStudioSolution solution;

  @Before
  public void initData() {
    VisualStudioProject vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getAssemblyName()).thenReturn("Project1");
    VisualStudioProject vsProject2 = mock(VisualStudioProject.class);
    when(vsProject2.getAssemblyName()).thenReturn("Project2");
    VisualStudioProject vsTestProject1 = mock(VisualStudioProject.class);
    when(vsTestProject1.getArtifact("Debug")).thenReturn(FAKE_ASSEMBLY_1);
    VisualStudioProject vsTestProject2 = mock(VisualStudioProject.class);
    when(vsTestProject2.getArtifact("Debug")).thenReturn(FAKE_ASSEMBLY_2);
    solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject1, vsProject2));
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsTestProject1, vsTestProject2));
  }

  @Test
  public void testToCommandForSolutionWithMinimumParams() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
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
    when(vsProject.getArtifact("Release")).thenReturn(FAKE_ASSEMBLY_2);
    solution = mock(VisualStudioSolution.class);
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsProject));

    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(new File("target/sonar/gallio-report-folder/gallio-report.XmL")); // we use here mixed case on purpose
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

  @Test
  public void testToCommandForSolutionWithPartCoverWithMinimumParams() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.setCoverageTool("PartCover");
    builder.setPartCoverInstallDirectory(PART_COVER_INSTALL_DIR);
    builder.setCoverageReportFile(GALLIO_COVERAGE_REPORT_FILE);
    builder.setWorkDir(WORK_DIR);
    Command command = builder.toCommand();

    assertThat(command.getExecutable(), endsWith("PartCover.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands.length, is(12));
    assertThat(commands[0], is("--target"));
    assertThat(commands[1], endsWith("Gallio.Echo.exe"));
    assertThat(commands[2], is("--target-work-dir"));
    assertThat(commands[3], is(WORK_DIR.getAbsolutePath()));
    assertThat(commands[4], is("--target-args"));
    assertThat(commands[5], startsWith("\\\"/r:IsolatedAppDomain\\\" \\\"/report-directory:"));
    assertThat(commands[5],
        containsString("gallio-report-folder\\\" \\\"/report-name-format:gallio-report\\\" \\\"/report-type:Xml\\\" \\\""));
    assertThat(commands[5], containsString("Fake1.assembly\\\" \\\""));
    assertThat(commands[5], endsWith("Fake2.assembly\\\""));
    assertThat(commands[6], is("--include"));
    assertThat(commands[7], is("[Project1]*"));
    assertThat(commands[8], is("--include"));
    assertThat(commands[9], is("[Project2]*"));
    assertThat(commands[10], is("--output"));
    assertThat(commands[11], endsWith("coverage-report.xml"));
  }

  @Test
  public void testToCommandForSolutionWithPartCoverWithMoreParams() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.setCoverageTool("PartCover");
    builder.setPartCoverInstallDirectory(PART_COVER_INSTALL_DIR);
    builder.setCoverageReportFile(GALLIO_COVERAGE_REPORT_FILE);
    builder.setWorkDir(WORK_DIR);
    builder.setCoverageExcludes("Foo, Bar");
    Command command = builder.toCommand();

    assertThat(command.getExecutable(), endsWith("PartCover.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands.length, is(16));
    assertThat(commands[10], is("--exclude"));
    assertThat(commands[11], is("Foo"));
    assertThat(commands[12], is("--exclude"));
    assertThat(commands[13], is("Bar"));
  }

  @Test
  public void testToCommandForSolutionWithNCoverParams() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.setCoverageTool("NCover");
    builder.setCoverageReportFile(GALLIO_COVERAGE_REPORT_FILE);
    Command command = builder.toCommand();

    assertThat(command.getExecutable(), endsWith("Gallio.Echo.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands.length, is(8));
    assertThat(commands[0], is("/r:NCover3"));
    assertThat(commands[1], endsWith("gallio-report-folder"));
    assertThat(commands[2], is("/report-name-format:gallio-report"));
    assertThat(commands[3], is("/report-type:Xml"));
    assertThat(commands[4], endsWith("Fake1.assembly"));
    assertThat(commands[5], endsWith("Fake2.assembly"));
    assertThat(commands[6], startsWith("/runner-property:NCoverCoverageFile="));
    assertThat(commands[6], endsWith("coverage-report.xml"));
    assertThat(commands[7], is("/runner-property:NCoverArguments=//ias Project1;Project2"));
  }

  @Test(expected = GallioException.class)
  public void testToCommandNoConfigFile() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testToCommandNoSolutionOrProject() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder((VisualStudioSolution) null);
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testToCommandUnexistingGallioExe() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(TestUtils.getResource("/Runner/UnexistingProgDir/Gallio.Echo.exe"));
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testToCommandNoTestAssembly() throws Exception {
    VisualStudioProject vsProject = mock(VisualStudioProject.class);
    when(vsProject.getArtifact("Debug")).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Unexisting.assembly"));
    solution = mock(VisualStudioSolution.class);
    when(solution.getTestProjects()).thenReturn(Lists.newArrayList(vsProject));
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(TestUtils.getResource("/Runner/FakeProg"));
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testToCommandUnexistingPartCoverDir() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.setCoverageTool("PartCover");
    builder.setPartCoverInstallDirectory(TestUtils.getResource("/Runner/UnexistingPartCoverDir"));
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testToCommandNoWorkDirSpecified() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.setCoverageTool("PartCover");
    builder.setPartCoverInstallDirectory(PART_COVER_INSTALL_DIR);
    builder.toCommand();
  }

  @Test(expected = GallioException.class)
  public void testToCommandNoCoverageReportFileSpecified() throws Exception {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(GALLIO_EXE);
    builder.setReportFile(GALLIO_REPORT_FILE);
    builder.setCoverageTool("PartCover");
    builder.setPartCoverInstallDirectory(PART_COVER_INSTALL_DIR);
    builder.setWorkDir(WORK_DIR);
    builder.toCommand();
  }

}
