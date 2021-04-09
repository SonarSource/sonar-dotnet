/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2021 SonarSource SA
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
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Collectors;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues;

import static com.sonar.it.vbnet.Tests.getIssues;
import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class TestProjectTest {

  @ClassRule
  public static final TemporaryFolder temp = TestUtils.createTempFolder();

  private static BuildResult buildResult;

  @BeforeClass
  public static void init() throws Exception {
    TestUtils.reset(ORCHESTRATOR);
    buildResult = Tests.analyzeProject(temp, "VbTestOnlyProjectTest", null);
  }

  @Test
  public void with_vbnet_only_test_should_not_populate_metrics() throws Exception {
    Path projectDir = Tests.projectDir(temp, "VbTestOnlyProjectTest");

    ScannerForMSBuild beginStep = TestUtils.createBeginStep("VbTestOnlyProjectTest", projectDir, "MyLib.Tests")
      .setProfile("vbnet_no_rule");

    ORCHESTRATOR.executeBuild(beginStep);

    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Restore,Rebuild");

    BuildResult buildResult = ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));

    assertThat(Tests.getComponent("VbTestOnlyProjectTest:UnitTest1.vb")).isNotNull();
    assertThat(getMeasureAsInt("VbTestOnlyProjectTest", "files")).isNull();
    assertThat(getMeasureAsInt("VbTestOnlyProjectTest", "lines")).isNull();
    assertThat(getMeasureAsInt("VbTestOnlyProjectTest", "ncloc")).isNull();

    assertThat(buildResult.getLogsLines(l -> l.contains("WARN")))
      .contains("WARN: SonarScanner for .NET detected only TEST files and no MAIN files for VB.NET in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");

    assertThat(buildResult.getLogsLines(l -> l.contains("INFO"))).contains("INFO: Found 1 MSBuild VB.NET project: 1 TEST project.");
    TestUtils.verifyGuiTestOnlyProjectAnalysisWarning(ORCHESTRATOR, buildResult, "VB.NET");
  }

  @Test
  public void issuesAreImportedForTestProject(){
    List<Issues.Issue> barIssues = getIssues("VbTestOnlyProjectTest:MyLib.Tests/UnitTest1.vb")
      .stream()
      .filter(x -> x.getRule().startsWith("vbnet:"))
      .collect(Collectors.toList());

    assertThat(barIssues).hasSize(2);

    assertThat(barIssues)
      .filteredOn(e -> e.getRule().equalsIgnoreCase("vbnet:S1125"))
      .hasOnlyOneElementSatisfying(e -> assertThat(e.getLine()).isEqualTo(13));
  }
}
