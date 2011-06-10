/*
 * .NET tools :: StyleCop Runner
 * Copyright (C) 2011 Jose Chillan, Alexandre Victoor and SonarSource
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

package org.sonar.dotnet.tools.stylecop;

import static org.hamcrest.Matchers.endsWith;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.Test;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

public class StyleCopRunnerTest {

  private VisualStudioSolution solution;
  private VisualStudioProject project;

  @Before
  public void initData() {
    project = mock(VisualStudioProject.class);
    when(project.getProjectFile()).thenReturn(new File("target/sonar/solution/project/project.csproj"));
    new File("target/sonar/solution").mkdirs();
    solution = mock(VisualStudioSolution.class);
    when(solution.getSolutionDir()).thenReturn(new File("target/sonar/solution"));
    when(solution.getSolutionFile()).thenReturn(new File("target/sonar/solution/solution.sln"));
  }

  @Test
  public void testCreateCommandBuilderForSolution() throws Exception {
    String fakeInstallDir = TestUtils.getResource("/Runner/Command").getAbsolutePath();
    StyleCopRunner runner = StyleCopRunner.create(fakeInstallDir, new File("dotnetInstallDir").getAbsolutePath(), new File(
        "target/sonar/tempFolder").getAbsolutePath());
    StyleCopCommandBuilder builder = runner.createCommandBuilder(solution);
    builder.setConfigFile(TestUtils.getResource("/Runner/Command/SimpleRules.StyleCop"));
    builder.setReportFile(new File("target/sonar/stylecop-report.xml"));
    assertThat(builder.toCommand().getExecutable(), endsWith("MSBuild.exe"));
  }

  @Test
  public void testCreateCommandBuilderForProject() throws Exception {
    String fakeInstallDir = TestUtils.getResource("/Runner/Command").getAbsolutePath();
    StyleCopRunner runner = StyleCopRunner.create(fakeInstallDir, new File("dotnetInstallDir").getAbsolutePath(), new File(
        "target/sonar/tempFolder").getAbsolutePath());
    StyleCopCommandBuilder builder = runner.createCommandBuilder(solution, project);
    builder.setConfigFile(TestUtils.getResource("/Runner/Command/SimpleRules.StyleCop"));
    builder.setReportFile(new File("target/sonar/stylecop-report.xml"));
    assertThat(builder.toCommand().getExecutable(), endsWith("MSBuild.exe"));
  }

}
