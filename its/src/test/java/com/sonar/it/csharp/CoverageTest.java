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
import com.sonar.orchestrator.build.BuildResult;

import java.io.IOException;
import java.nio.file.Path;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getMeasure;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class CoverageTest {

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Before
  public void init() {
    orchestrator.resetData();
  }

  @Test
  public void should_not_import_coverage_without_report() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject();
    assertThat(buildResult.getLogs()).doesNotContain("C# Tests Coverage Report Import");

    org.sonarqube.ws.WsMeasures.Measure linesToCover = getMeasure("CoverageTest", "lines_to_cover");
    org.sonarqube.ws.WsMeasures.Measure uncoveredLines = getMeasure("CoverageTest", "uncovered_lines");

    assertThat(linesToCover.getValue()).isEqualTo("2");
    assertThat(uncoveredLines.getValue()).isEqualTo("2");
}

  @Test
  public void ncover3() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.ncover3.reportsPaths", "reports/ncover3.nccov");

    assertThat(buildResult.getLogs()).contains("C# Tests Coverage Report Import");
    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void open_cover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.opencover.reportsPaths", "reports/opencover.xml");

    assertThat(buildResult.getLogs()).contains("C# Tests Coverage Report Import");
    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(0);
  }

  @Test
  public void dotcover() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.dotcover.reportsPaths", "reports/dotcover.html");

    assertThat(buildResult.getLogs()).contains("C# Tests Coverage Report Import");
    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void visual_studio() throws Exception {
    BuildResult buildResult = analyzeCoverageTestProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(buildResult.getLogs()).contains("C# Tests Coverage Report Import");
    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
    assertThat(getMeasureAsInt("CoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void no_coverage_on_tests() throws Exception {
    Path projectDir = Tests.projectDir(temp, "NoCoverageOnTests");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("NoCoverageOnTests")
      .setProjectVersion("1.0")
      .setProfile("no_rule")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));

    assertThat(getMeasureAsInt("NoCoverageOnTests", "files")).isEqualTo(2); // Only main files are counted
    String unitTestComponentId = TestUtils.hasModules(ORCHESTRATOR) ? "NoCoverageOnTests:NoCoverageOnTests:8A3B715A-6E95-4BC1-93C6-A59E9D3F5D5C:UnitTest1.cs" : "NoCoverageOnTests:MyLib.Tests/UnitTest1.cs";
    assertThat(Tests.getComponent(unitTestComponentId)).isNotNull();
    assertThat(getMeasureAsInt("NoCoverageOnTests", "lines_to_cover")).isNull();
    assertThat(getMeasureAsInt("NoCoverageOnTests", "uncovered_lines")).isNull();
  }

  @Test
  public void should_support_wildcard_patterns() throws Exception {
    analyzeCoverageTestProject("sonar.cs.ncover3.reportsPaths", "reports/*.nccov");

    assertThat(getMeasureAsInt("CoverageTest", "lines_to_cover")).isEqualTo(2);
  }

  private BuildResult analyzeCoverageTestProject(String... keyValues) throws IOException {
    Path projectDir = Tests.projectDir(temp, "CoverageTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setProfile("no_rule")
      .setProperties(keyValues));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    return orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));
  }

}
