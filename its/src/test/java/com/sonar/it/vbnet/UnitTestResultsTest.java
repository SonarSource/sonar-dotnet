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

import static com.sonar.it.vbnet.Tests.getMeasure;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class UnitTestResultsTest {

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Before
  public void init() {
    orchestrator.resetData();
  }

  @Test
  public void should_not_import_unit_test_results_without_report() throws Exception {
    analyzeTestProject();

    assertThat(getMeasure("VbUnitTestResultsTest", "tests")).isNull();
    assertThat(getMeasure("VbUnitTestResultsTest", "test_errors")).isNull();
    assertThat(getMeasure("VbUnitTestResultsTest", "test_failures")).isNull();
    assertThat(getMeasure("VbUnitTestResultsTest", "skipped_tests")).isNull();
  }

  private void analyzeTestProject(String... keyValues) throws IOException {
    Path projectDir = Tests.projectDir(temp, "VbUnitTestResultsTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("VbUnitTestResultsTest")
      .setProjectName("VbUnitTestResultsTest")
      .setProjectVersion("1.0")
      .setProfile("vbnet_no_rule")
      .setProperties(keyValues));

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));
  }

  @Test
  public void vstest() throws Exception {
    analyzeTestProject("sonar.vbnet.vstest.reportsPaths", "reports/vstest.trx");

    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "tests")).isEqualTo(42);
    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "test_errors")).isEqualTo(1);
    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "test_failures")).isEqualTo(10);
    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "skipped_tests")).isEqualTo(2);
  }

  @Test
  public void nunit() throws Exception {
    analyzeTestProject("sonar.vbnet.nunit.reportsPaths", "reports/nunit.xml");

    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "tests")).isEqualTo(200);
    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "test_errors")).isEqualTo(30);
    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "test_failures")).isEqualTo(20);
    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "skipped_tests")).isEqualTo(9);
  }

  @Test
  public void should_support_wildcard_patterns() throws Exception {
    analyzeTestProject("sonar.vbnet.vstest.reportsPaths", "reports/*.trx");

    assertThat(getMeasureAsInt("VbUnitTestResultsTest", "tests")).isEqualTo(42);
  }
}
