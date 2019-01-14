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
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import com.sonar.orchestrator.Orchestrator;

import java.io.IOException;
import java.nio.file.Path;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
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
  public void without_coverage_report_still_count_lines_to_cover() throws Exception {
    analyzeCoverageTestProject();

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(4);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(4);
  }

  @Test
  public void ncover3() throws Exception {
    analyzeCoverageTestProject("sonar.vbnet.ncover3.reportsPaths", "reports/ncover3.nccov");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(6);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void open_cover() throws Exception {
    analyzeCoverageTestProject("sonar.vbnet.opencover.reportsPaths", "reports/opencover.xml");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(9);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void dotcover() throws Exception {
    analyzeCoverageTestProject("sonar.vbnet.dotcover.reportsPaths", "reports/dotcover.html");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void visual_studio() throws Exception {
    analyzeCoverageTestProject("sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt("VbCoverageTest", "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void no_coverage_on_tests() throws Exception {
    Path projectDir = Tests.projectDir(temp, "VbNoCoverageOnTests");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("VbNoCoverageOnTests")
      .setProjectVersion("1.0")
      .setProfile("vbnet_no_rule")
      .setProperty("sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));

    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "files")).isEqualTo(2); // Only main files are counted
    String class1VbComponentId = TestUtils.hasModules(ORCHESTRATOR) ? "VbNoCoverageOnTests:VbNoCoverageOnTests:7E4004A5-75CF-475C-9922-589EF95517D8:Class1.vb" : "VbNoCoverageOnTests:MyLib/Class1.vb";
    assertThat(Tests.getComponent(class1VbComponentId)).isNotNull();
    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "lines_to_cover")).isNull();
    assertThat(getMeasureAsInt("VbNoCoverageOnTests", "uncovered_lines")).isNull();
  }

  @Test
  public void should_support_wildcard_patterns() throws Exception {
    analyzeCoverageTestProject("sonar.vbnet.ncover3.reportsPaths", "reports/*.nccov");

    assertThat(getMeasureAsInt("VbCoverageTest", "lines_to_cover")).isEqualTo(6);
  }

  private void analyzeCoverageTestProject(String... keyValues) throws IOException {
    Path projectDir = Tests.projectDir(temp, "VbCoverageTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("VbCoverageTest")
      .setProjectVersion("1.0")
      .setProfile("vbnet_no_rule")
      .setProperties(keyValues));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));
  }

  @Test
  public void mix_vscoverage() throws IOException {
    analyzeCoverageMixProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml",
      "sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "lines_to_cover")).isEqualTo(10);
    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "uncovered_lines")).isEqualTo(4);

    assertThat(getMeasureAsInt(getProgramCsComponentId(), "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(getProgramCsComponentId(), "uncovered_lines")).isEqualTo(2);

    assertThat(getMeasureAsInt(getModule1VbComponentId(), "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(getModule1VbComponentId(), "uncovered_lines")).isEqualTo(2);
  }

  @Test
  public void mix_only_cs_vscoverage() throws IOException {
    analyzeCoverageMixProject("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "lines_to_cover")).isEqualTo(6);
    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "uncovered_lines")).isEqualTo(3);

    assertThat(getMeasureAsInt(getProgramCsComponentId(), "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(getProgramCsComponentId(), "uncovered_lines")).isEqualTo(2);

    assertThat(getMeasureAsInt(getModule1VbComponentId(), "lines_to_cover")).isEqualTo(1);
    assertThat(getMeasureAsInt(getModule1VbComponentId(), "uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void mix_only_vbnet_vscoverage() throws IOException {
    analyzeCoverageMixProject("sonar.vbnet.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "lines_to_cover")).isEqualTo(6);
    assertThat(getMeasureAsInt("CSharpVBNetCoverage", "uncovered_lines")).isEqualTo(3);

    assertThat(getMeasureAsInt(getProgramCsComponentId(), "lines_to_cover")).isEqualTo(1);
    assertThat(getMeasureAsInt(getProgramCsComponentId(), "uncovered_lines")).isEqualTo(1);

    assertThat(getMeasureAsInt(getModule1VbComponentId(), "lines_to_cover")).isEqualTo(5);
    assertThat(getMeasureAsInt(getModule1VbComponentId(), "uncovered_lines")).isEqualTo(2);
  }

  private void analyzeCoverageMixProject(String... keyValues) throws IOException {
    Path projectDir = Tests.projectDir(temp, "CSharpVBNetCoverage");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("CSharpVBNetCoverage")
      .setProjectVersion("1.0")
      .setProperties(keyValues));

    TestUtils.runNuGet(orchestrator, projectDir, "restore");
    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));
  }

  private static final String getProgramCsComponentId() { return TestUtils.hasModules(orchestrator) ? "CSharpVBNetCoverage:CSharpVBNetCoverage:2EC3A59D-240B-498F-BF1F-EB2A84092718:Program.cs" : "CSharpVBNetCoverage:CSharpConsoleApp/Program.cs"; }

  private static final String getModule1VbComponentId() { return TestUtils.hasModules(orchestrator) ? "CSharpVBNetCoverage:CSharpVBNetCoverage:4745F19D-A6DB-4FBB-8A15-DDCA487A035E:Module1.vb" : "CSharpVBNetCoverage:VBNetConsoleApp/Module1.vb"; }
}
