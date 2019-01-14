/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2019 SonarSource SA
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
import java.nio.file.Path;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class DoNotAnalyzeTestFilesTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void init() {
    orchestrator.resetData();
  }

  @Test
  public void should_not_increment_test() throws Exception {
    Path projectDir = Tests.projectDir(temp, "DoNotAnalyzeTestFilesTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("DoNotAnalyzeTestFilesTest")
      .setProjectName("DoNotAnalyzeTestFilesTest")
      .setProjectVersion("1.0")
      .setProfile("no_rule")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));

    String unitTestComponentId = TestUtils.hasModules(ORCHESTRATOR) ? "DoNotAnalyzeTestFilesTest:DoNotAnalyzeTestFilesTest:8A3B715A-6E95-4BC1-93C6-A59E9D3F5D5C:UnitTest1.cs" : "DoNotAnalyzeTestFilesTest:UnitTest1.cs";
    assertThat(Tests.getComponent(unitTestComponentId)).isNotNull();
    assertThat(getMeasureAsInt("DoNotAnalyzeTestFilesTest", "files")).isNull();
    assertThat(getMeasureAsInt("DoNotAnalyzeTestFilesTest", "lines")).isNull();
    assertThat(getMeasureAsInt("DoNotAnalyzeTestFilesTest", "ncloc")).isNull();
  }

}
