/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2020 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.nio.file.Path;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class DoNotAnalyzeTestFilesTest {

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  private static final String PROJECT = "DoNotAnalyzeTestFilesTest";

  @Before
  public void init() {
    TestUtils.reset(orchestrator);
  }

  @Test
  public void should_not_increment_test() throws Exception {
    Path projectDir = Tests.projectDir(temp, PROJECT);

    ScannerForMSBuild beginStep = TestUtils.createBeginStep(PROJECT, projectDir, "MyLib.Tests")
      .setProfile("no_rule")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    orchestrator.executeBuild(beginStep);

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.createEndStep(projectDir));

    assertThat(Tests.getComponent("DoNotAnalyzeTestFilesTest:UnitTest1.cs")).isNotNull();
    assertThat(getMeasureAsInt(PROJECT, "files")).isNull();
    assertThat(getMeasureAsInt(PROJECT, "lines")).isNull();
    assertThat(getMeasureAsInt(PROJECT, "ncloc")).isNull();
  }
}
