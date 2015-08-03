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
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.sonar.wsclient.Sonar;
import org.sonar.wsclient.services.ResourceQuery;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;

public class UnitTestResultsTest {

  @ClassRule
  public static Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @BeforeClass
  public static void init() throws Exception {
    orchestrator.resetData();
  }

  @Test
  public void should_not_import_unit_test_results_without_report() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/UnitTestResultsTest/"))
      .setProjectKey("UnitTestResultsTest")
      .setProjectName("UnitTestResultsTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "tests"))).isNull();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "test_errors"))).isNull();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "test_failures"))).isNull();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "skipped_tests"))).isNull();
  }

  @Test
  public void vstest() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/UnitTestResultsTest/"))
      .setProjectKey("UnitTestResultsTest")
      .setProjectName("UnitTestResultsTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.vstest.reportsPaths", "reports/vstest.trx")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "tests")).getMeasureIntValue("tests")).isEqualTo(32);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "test_errors")).getMeasureIntValue("test_errors")).isEqualTo(1);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "test_failures")).getMeasureIntValue("test_failures")).isEqualTo(10);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "skipped_tests")).getMeasureIntValue("skipped_tests")).isEqualTo(7);
  }

  @Test
  public void nunit() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/UnitTestResultsTest/"))
      .setProjectKey("UnitTestResultsTest")
      .setProjectName("UnitTestResultsTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.nunit.reportsPaths", "reports/nunit.xml")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "tests")).getMeasureIntValue("tests")).isEqualTo(196);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "test_errors")).getMeasureIntValue("test_errors")).isEqualTo(30);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "test_failures")).getMeasureIntValue("test_failures")).isEqualTo(20);
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "skipped_tests")).getMeasureIntValue("skipped_tests")).isEqualTo(7);
  }

  @Test
  public void should_support_wildcard_patterns() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/UnitTestResultsTest/"))
      .setProjectKey("UnitTestResultsTest")
      .setProjectName("UnitTestResultsTest")
      .setProjectVersion("1.0")
      .setSourceDirs("src")
      .setTestDirs("tests")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProperty("sonar.cs.vstest.reportsPaths", "reports/*.trx")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("UnitTestResultsTest", "tests")).getMeasureIntValue("tests")).isEqualTo(32);
  }

}
