/*
 * .NET tools :: Gendarme Runner
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
package org.sonar.dotnet.tools.gendarme;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.notNullValue;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.Test;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;
import com.google.common.collect.Sets;

public class GendarmeRunnerTest {

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;
  private GendarmeRunner runner;
  private String fakeExecPath;

  @Before
  public void initData() throws Exception {
    vsProject = mock(VisualStudioProject.class);
    solution = mock(VisualStudioSolution.class);
    when(vsProject.getGeneratedAssemblies("Debug")).thenReturn(
        Sets.newHashSet(TestUtils.getResource("/runner/FakeAssemblies/Fake1.assembly")));
    when(vsProject.getArtifactDirectory("Debug")).thenReturn(TestUtils.getResource("/runner/FakeAssemblies"));
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject));

    fakeExecPath = TestUtils.getResource("/runner/FakeProg/gendarme.exe").getAbsolutePath();
    runner = GendarmeRunner.create(TestUtils.getResource("/runner/FakeProg").getAbsolutePath(),
        new File("target/sonar/tempFolder").getAbsolutePath());
  }

  @Test
  public void testCreateCommandBuilderForSolution() throws Exception {
    GendarmeCommandBuilder builder = runner.createCommandBuilder(solution);
    builder.setConfigFile(TestUtils.getResource("/runner/FakeGendarmeConfigFile.xml"));
    builder.setReportFile(new File("gendarme-report.xml"));
    assertThat(builder.toCommand().getExecutable(), is(fakeExecPath));
  }

  @Test
  public void testCreateCommandBuilderForProject() throws Exception {
    GendarmeCommandBuilder builder = runner.createCommandBuilder(vsProject);
    builder.setConfigFile(TestUtils.getResource("/runner/FakeGendarmeConfigFile.xml"));
    builder.setReportFile(new File("gendarme-report.xml"));
    assertThat(builder.toCommand().getExecutable(), is(fakeExecPath));
  }

  @Test
  public void testDeleteSilverlightFile() throws Exception {
    when(vsProject.isSilverlightProject()).thenReturn(true);
    GendarmeCommandBuilder builder = runner.createCommandBuilder(vsProject);
    builder.setConfigFile(TestUtils.getResource("/runner/FakeGendarmeConfigFile.xml"));
    builder.setReportFile(new File("gendarme-report.xml"));
    builder.setSilverlightFolder(TestUtils.getResource("/runner/SilverlightFolder"));
    builder.toCommand();

    File copiedSilverlightAssembly = TestUtils.getResource("/runner/FakeAssemblies/mscorlib.dll");
    assertThat(copiedSilverlightAssembly, notNullValue());
    assertTrue(copiedSilverlightAssembly.isFile());

    runner.cleanupFiles("Debug");
    assertFalse(copiedSilverlightAssembly.exists());
  }

}
