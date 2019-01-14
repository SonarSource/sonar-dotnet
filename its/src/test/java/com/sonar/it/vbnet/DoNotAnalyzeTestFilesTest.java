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
package com.sonar.it.vbnet;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import java.nio.file.Path;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class DoNotAnalyzeTestFilesTest {

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Before
  public void init() {
    orchestrator.resetData();
  }

  @Test
  public void should_not_increment_test() throws Exception {
    Path projectDir = Tests.projectDir(temp, "VbDoNotAnalyzeTestFilesTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("VbDoNotAnalyzeTestFilesTest")
      .setProjectName("VbDoNotAnalyzeTestFilesTest")
      .setProjectVersion("1.0")
      .setProfile("vbnet_no_rule")
      .setProperty("sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));

    String unitTest1ComponentId = TestUtils.hasModules(ORCHESTRATOR) ? "VbDoNotAnalyzeTestFilesTest:VbDoNotAnalyzeTestFilesTest:039CD4D5-8C38-47EB-AE09-F2457DE61EFC:UnitTest1.vb" : "VbDoNotAnalyzeTestFilesTest:UnitTest1.vb";
    assertThat(Tests.getComponent(unitTest1ComponentId)).isNotNull();
    assertThat(getMeasureAsInt("VbDoNotAnalyzeTestFilesTest", "files")).isNull();
    assertThat(getMeasureAsInt("VbDoNotAnalyzeTestFilesTest", "lines")).isNull();
    assertThat(getMeasureAsInt("VbDoNotAnalyzeTestFilesTest", "ncloc")).isNull();
  }

}
