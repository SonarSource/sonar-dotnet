/*
 * .NET tools :: DependencyParser Runner
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
package org.sonar.dotnet.tools.dependencyparser;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.command.Command;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.test.TestUtils;

public class DependencyParserRunnerTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();;

  private static File fakeDependencyParserInstallDir;
  private static File fakeDependencyParserReportFile;
  private VisualStudioProject vsProject;
  private DependencyParserRunner runner;

  @BeforeClass
  public static void initStatic() throws Exception {
    fakeDependencyParserInstallDir = TestUtils.getResource("/Runner/FakeProg");
    fakeDependencyParserReportFile = new File("target/sonar/Deps/deps-report.xml");
  }

  @Before
  public void initData() throws Exception {
    vsProject = mock(VisualStudioProject.class);
    when(vsProject.getArtifact("Debug")).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Fake1.assembly"));
    when(vsProject.getDirectory()).thenReturn(TestUtils.getResource("/Runner"));
    runner = DependencyParserRunner.create(fakeDependencyParserInstallDir.getAbsolutePath(), new File("target/sonar/tempFolder").getAbsolutePath());
  }

  @Test
  public void testCreateCommandBuilderForProject() throws Exception {
    DependencyParserCommandBuilder builder = runner.createCommandBuilder(null, vsProject);
    builder.setReportFile(fakeDependencyParserReportFile);
    assertThat(builder.toCommand().getExecutable(), is(new File(fakeDependencyParserInstallDir, "DependencyParser.exe").getAbsolutePath()));
  }

  @Test
  public void testReturnCode0() throws Exception {
    DependencyParserCommandBuilder builder = mock(DependencyParserCommandBuilder.class);

    // 0 - everything went fine
    Command command = createReturnCodeCommand();
    command.addArgument("0");
    when(builder.toCommand()).thenReturn(command);
    runner.execute(builder, 1);
  }

  @Test
  public void testReturnCode1() throws Exception {
    DependencyParserCommandBuilder builder = mock(DependencyParserCommandBuilder.class);

    // 1 - and error occurred
    Command command = createReturnCodeCommand();
    command.addArgument("1");
    when(builder.toCommand()).thenReturn(command);

    thrown.expect(DependencyParserException.class);
    thrown.expectMessage("execution was interrupted by a non-handled exception");
    runner.execute(builder, 1);
  }

  @Test
  public void testReturnCode2() throws Exception {
    DependencyParserCommandBuilder builder = mock(DependencyParserCommandBuilder.class);

    // 2 - and I/O error occurred
    Command command = createReturnCodeCommand();
    command.addArgument("2");
    when(builder.toCommand()).thenReturn(command);

    thrown.expect(DependencyParserException.class);
    thrown.expectMessage("execution was interrupted by I/O errors (e.g. missing files).");
    runner.execute(builder, 1);
  }

  @Test
  public void testReturnCode3() throws Exception {
    DependencyParserCommandBuilder builder = mock(DependencyParserCommandBuilder.class);

    // 3 - and I/O error occurred
    Command command = createReturnCodeCommand();
    command.addArgument("3");
    when(builder.toCommand()).thenReturn(command);

    thrown.expect(DependencyParserException.class);
    thrown.expectMessage("errors found in the (default or user supplied) configuration files.");
    runner.execute(builder, 1);
  }

  protected Command createReturnCodeCommand() {
    Command command = Command.create("java");
    command.addArgument("-jar");
    command.addArgument(TestUtils.getResource("/Lib/ReturnCode.jar").getAbsolutePath());
    return command;
  }

}
