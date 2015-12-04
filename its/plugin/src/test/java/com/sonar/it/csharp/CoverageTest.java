/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011 SonarSource
 * sonarqube@googlegroups.com
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
package com.sonar.it.csharp;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.SonarRunner;
import org.junit.Assume;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.sonar.wsclient.Sonar;
import org.sonar.wsclient.services.ResourceQuery;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;

public class CoverageTest {

  @ClassRule
  public static Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @BeforeClass
  public static void init() throws Exception {
    orchestrator.resetData();
  }

  @Test
  public void should_not_import_coverage_without_report() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/CoverageTest/"))
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "lines_to_cover"))).isNull();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "uncovered_lines"))).isNull();
  }

  @Test
  public void ncover3() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/CoverageTest/"))
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.ncover3.reportsPaths", "reports/ncover3.nccov")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "lines_to_cover")).getMeasureIntValue("lines_to_cover")).isEqualTo(2);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "uncovered_lines")).getMeasureIntValue("uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void open_cover() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/CoverageTest/"))
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.opencover.reportsPaths", "reports/opencover.xml")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "lines_to_cover")).getMeasureIntValue("lines_to_cover")).isEqualTo(2);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "uncovered_lines")).getMeasureIntValue("uncovered_lines")).isEqualTo(0);
  }

  @Test
  public void dotcover() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/CoverageTest/"))
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.dotcover.reportsPaths", "reports/dotcover.html")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "lines_to_cover")).getMeasureIntValue("lines_to_cover")).isEqualTo(2);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "uncovered_lines")).getMeasureIntValue("uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void visual_studio() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/CoverageTest/"))
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "lines_to_cover")).getMeasureIntValue("lines_to_cover")).isEqualTo(2);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "uncovered_lines")).getMeasureIntValue("uncovered_lines")).isEqualTo(1);
  }

  @Test
  public void no_coverage_on_tests() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/NoCoverageOnTests/"))
      .setProjectKey("NoCoverageOnTests")
      .setProjectName("NoCoverageOnTests")
      .setProjectVersion("1.0")
      .setSourceDirs("src")
      .setTestDirs("tests")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("NoCoverageOnTests", "lines_to_cover"))).isNull();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("NoCoverageOnTests", "uncovered_lines"))).isNull();
  }

  @Test
  public void should_support_wildcard_patterns() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/CoverageTest/"))
      .setProjectKey("CoverageTest")
      .setProjectName("CoverageTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.ncover3.reportsPaths", "reports/*.nccov")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("CoverageTest", "lines_to_cover")).getMeasureIntValue("lines_to_cover")).isEqualTo(2);
  }

}
