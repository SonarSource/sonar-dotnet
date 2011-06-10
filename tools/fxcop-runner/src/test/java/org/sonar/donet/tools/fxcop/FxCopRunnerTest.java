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
package org.sonar.donet.tools.fxcop;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;
import com.google.common.collect.Sets;

public class FxCopRunnerTest {

  private static File fakeFxCopExecutable;
  private static File fakeFxCopConfigFile;
  private static File fakeFxCopReportFile;
  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;

  @BeforeClass
  public static void initStatic() throws Exception {
    fakeFxCopExecutable = TestUtils.getResource("/Runner/FakeProg/FxCopCmd.exe");
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
  public void testCreateCommandBuilderForSolution() throws Exception {
    FxCopRunner runner = FxCopRunner.create(fakeFxCopExecutable.getAbsolutePath());
    FxCopCommandBuilder builder = runner.createCommandBuilder(solution);
    builder.setConfigFile(fakeFxCopConfigFile);
    builder.setReportFile(fakeFxCopReportFile);
    assertThat(builder.toCommand().getExecutable(), is(fakeFxCopExecutable.getAbsolutePath()));
  }

  @Test
  public void testCreateCommandBuilderForProject() throws Exception {
    FxCopRunner runner = FxCopRunner.create(fakeFxCopExecutable.getAbsolutePath());
    FxCopCommandBuilder builder = runner.createCommandBuilder(vsProject);
    builder.setConfigFile(fakeFxCopConfigFile);
    builder.setReportFile(fakeFxCopReportFile);
    assertThat(builder.toCommand().getExecutable(), is(fakeFxCopExecutable.getAbsolutePath()));
  }

}
