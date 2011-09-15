/*
 * .NET tools :: FxCop Runner
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
package org.sonar.dotnet.tools.fxcop;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.fail;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.utils.command.Command;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;
import com.google.common.collect.Sets;

public class FxCopRunnerTest {

  private static File fakeFxCopInstallDir;
  private static File fakeFxCopConfigFile;
  private static File fakeFxCopReportFile;
  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;

  @BeforeClass
  public static void initStatic() throws Exception {
    fakeFxCopInstallDir = TestUtils.getResource("/Runner/FakeProg");
    fakeFxCopConfigFile = TestUtils.getResource("/Runner/FakeFxCopConfigFile.xml");
    fakeFxCopReportFile = new File("target/sonar/FxCop/fxcop-report.xml");
  }

  @Before
  public void initData() {
    solution = mock(VisualStudioSolution.class);
    vsProject = mock(VisualStudioProject.class);
    when(vsProject.getGeneratedAssemblies("Debug")).thenReturn(
        Sets.newHashSet(TestUtils.getResource("/Runner/FakeAssemblies/Fake1.assembly")));
    when(vsProject.getDirectory()).thenReturn(TestUtils.getResource("/Runner"));
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject));
    when(solution.getSolutionDir()).thenReturn(TestUtils.getResource("/Runner"));
  }

  @Test
  public void testCreateCommandBuilderForProject() throws Exception {
    FxCopRunner runner = FxCopRunner.create(fakeFxCopInstallDir.getAbsolutePath());
    FxCopCommandBuilder builder = runner.createCommandBuilder(solution, vsProject);
    builder.setConfigFile(fakeFxCopConfigFile);
    builder.setReportFile(fakeFxCopReportFile);
    assertThat(builder.toCommand().getExecutable(), is(new File(fakeFxCopInstallDir, "FxCopCmd.exe").getAbsolutePath()));
  }

  @Test
  public void testFxCopValidReturnCodes() throws Exception {
    FxCopRunner runner = FxCopRunner.create("");
    FxCopCommandBuilder builder = mock(FxCopCommandBuilder.class);

    // 0 - everything went fine
    Command command = createReturnCodeCommand();
    command.addArgument("0");
    when(builder.toCommand()).thenReturn(command);
    runner.execute(builder, 1);

    // 512 - missing indirect assembly reference
    // Note: on UNIX systems, exit codes range from 0 to 255 (unsigned 8-bit integer), but this test passes as 512
    // loops to 0 for an unsigned 8-bit integer.
    command = createReturnCodeCommand();
    command.addArgument("512");
    when(builder.toCommand()).thenReturn(command);
    runner.execute(builder, 1);

    // any other code should fail
    command = createReturnCodeCommand();
    command.addArgument("13");
    when(builder.toCommand()).thenReturn(command);
    try {
      runner.execute(builder, 1);
      fail("This exit code should not be considered as valid.");
    } catch (Exception e) {
      // This is normal
    }
  }

  protected Command createReturnCodeCommand() {
    Command command = Command.create("java");
    command.addArgument("-jar");
    command.addArgument(TestUtils.getResource("/Lib/ReturnCode.jar").getAbsolutePath());
    return command;
  }

}
