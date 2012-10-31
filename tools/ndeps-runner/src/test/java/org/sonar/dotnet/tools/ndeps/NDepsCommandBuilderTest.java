/*
 * .NET tools :: NDeps Runner
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
package org.sonar.dotnet.tools.ndeps;

import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;

import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.command.Command;
import org.sonar.test.TestUtils;

import java.io.File;

import static org.hamcrest.Matchers.endsWith;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class NDepsCommandBuilderTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  private static File nDepsExecutable;
  private static File nDepsReportFile;
  private VisualStudioProject vsProject;
  private NDepsCommandBuilder nDepsCommandBuilder;

  @BeforeClass
  public static void initStatic() throws Exception {
    nDepsExecutable = TestUtils.getResource("/Runner/FakeProg/DependencyParser.exe");
    nDepsReportFile = new File("target/sonar/Deps/deps-report.xml");
  }

  @Before
  public void init() throws Exception {
    vsProject = mock(VisualStudioProject.class);
    when(vsProject.getArtifact("Debug", null)).thenReturn(TestUtils.getResource("/Runner/FakeAssemblies/Fake1.assembly"));
    nDepsCommandBuilder = NDepsCommandBuilder.createBuilder(null, vsProject);
    nDepsCommandBuilder.setExecutable(nDepsExecutable);
    nDepsCommandBuilder.setReportFile(nDepsReportFile);
  }

  @Test
  public void testToCommandForVSProject() throws Exception {
    Command command = nDepsCommandBuilder.toCommand();
    assertThat(toUnixStyle(command.getExecutable()), endsWith("/Runner/FakeProg/DependencyParser.exe"));
    String[] commands = command.getArguments().toArray(new String[] {});
    assertThat(commands[0], is("-a"));
    assertThat(commands[1], endsWith("Fake1.assembly"));
    assertThat(commands[2], is("-o"));
    assertThat(commands[3], endsWith("deps-report.xml"));
  }

  @Test
  public void testToCommandWithNoAssembly() throws Exception {
    when(vsProject.getArtifact("Debug", null)).thenReturn(null);

    thrown.expect(NDepsException.class);
    thrown.expectMessage("Assembly to scan not found for project");
    nDepsCommandBuilder.toCommand();
  }

  @Test
  public void testToCommandWithUnexistingAssembly() throws Exception {
    when(vsProject.getArtifact("Debug", null)).thenReturn(new File("target/sonar/Deps/unexisting-assembly.dll"));

    thrown.expect(NDepsException.class);
    thrown.expectMessage("Assembly to scan not found for project");
    nDepsCommandBuilder.toCommand();
  }

  private String toUnixStyle(String path) {
    return path.replaceAll("\\\\", "/");
  }

}
