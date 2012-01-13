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

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;
import java.util.List;

import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class GallioRunnerTest {

  private static GallioRunner runner;
  private static String fakeExecInstallPath;
  private static String workDir;
  private static VisualStudioSolution solution;
  private static List<File> testAsssemblies;

  @BeforeClass
  public static void initData() {
    solution = mock(VisualStudioSolution.class);
    testAsssemblies = Lists.newArrayList(
        TestUtils.getResource("/Runner/FakeAssemblies/Fake1.assembly"),
        TestUtils.getResource("/Runner/FakeAssemblies/Fake2.assembly")
    );

    fakeExecInstallPath = TestUtils.getResource("/Runner/FakeProg/Gallio").getAbsolutePath();
    workDir = TestUtils.getResource("/Runner").getAbsolutePath();
    runner = GallioRunner.create(fakeExecInstallPath, workDir, false);
  }

  @Test
  public void testCreateCommandBuilderForSolution() throws Exception {
    GallioCommandBuilder builder = runner.createCommandBuilder(solution);
    builder.setReportFile(new File("target/sonar/gallio-report-folder/gallio-report.xml"));
    builder.setTestAssemblies(testAsssemblies);
    assertThat(builder.toCommand().getExecutable(), is(new File(fakeExecInstallPath, "bin/Gallio.Echo.exe").getAbsolutePath()));
  }

}
