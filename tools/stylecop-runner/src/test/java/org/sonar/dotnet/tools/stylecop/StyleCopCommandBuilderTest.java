/*
 * .NET tools :: StyleCop Runner
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
package org.sonar.dotnet.tools.stylecop;

import static org.hamcrest.Matchers.endsWith;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.utils.command.Command;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

public class StyleCopCommandBuilderTest {

  private static VisualStudioSolution solution;
  private static VisualStudioProject project;

  @BeforeClass
  public static void initData() {
    project = mock(VisualStudioProject.class);
    when(project.getProjectFile()).thenReturn(new File("target/sonar/solution/project/project.csproj"));
    new File("target/sonar/solution").mkdirs();
    solution = mock(VisualStudioSolution.class);
    when(solution.getSolutionDir()).thenReturn(new File("target/sonar/solution"));
    when(solution.getSolutionFile()).thenReturn(new File("target/sonar/solution/solution.sln"));
  }

  @Test
  public void testToCommandForSolution() throws Exception {
    StyleCopCommandBuilder styleCopCommandBuilder = StyleCopCommandBuilder.createBuilder(solution);
    styleCopCommandBuilder.setDotnetSdkDirectory(new File("DotnetSdkDir"));
    styleCopCommandBuilder.setStyleCopFolder(new File("StyleCopDir"));
    styleCopCommandBuilder.setConfigFile(TestUtils.getResource("/Runner/Command/SimpleRules.StyleCop"));
    styleCopCommandBuilder.setReportFile(new File("target/sonar/report.xml"));
    Command command = styleCopCommandBuilder.toCommand();

    assertThat(command.getExecutable(), endsWith("MSBuild.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands[0], endsWith("solution"));
    assertThat(commands[1], is("/target:StyleCopLaunch"));
    assertThat(commands[2], is("/noconsolelogger"));
    assertThat(commands[3], endsWith("stylecop-msbuild.xml"));

    File report = new File("target/sonar/stylecop-msbuild.xml");
    assertTrue(report.exists());
    report.delete();
  }

  @Test
  public void testToCommandForProject() throws Exception {
    StyleCopCommandBuilder styleCopCommandBuilder = StyleCopCommandBuilder.createBuilder(solution, project);
    styleCopCommandBuilder.setDotnetSdkDirectory(new File("DotnetSdkDir"));
    styleCopCommandBuilder.setStyleCopFolder(new File("StyleCopDir"));
    styleCopCommandBuilder.setConfigFile(TestUtils.getResource("/Runner/Command/SimpleRules.StyleCop"));
    styleCopCommandBuilder.setReportFile(new File("target/sonar/report.xml"));
    Command command = styleCopCommandBuilder.toCommand();

    assertThat(command.getExecutable(), endsWith("MSBuild.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands[0], endsWith("solution"));
    assertThat(commands[1], is("/target:StyleCopLaunch"));
    assertThat(commands[2], is("/noconsolelogger"));
    assertThat(commands[3], endsWith("stylecop-msbuild.xml"));

    File report = new File("target/sonar/stylecop-msbuild.xml");
    assertTrue(report.exists());
    report.delete();
  }

  @Test(expected = IllegalStateException.class)
  public void testWithUnexistingStyleCopConfigFile() throws Exception {
    StyleCopCommandBuilder styleCopCommandBuilder = StyleCopCommandBuilder.createBuilder(solution);
    styleCopCommandBuilder.toCommand();
  }

}
