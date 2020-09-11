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
package com.sonar.it.vbnet;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static com.sonar.it.vbnet.Tests.getMeasure;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class UnitTestResultsTest {

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String PROJECT = "VbUnitTestResultsTest";

  @Before
  public void init(){
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void should_not_import_unit_test_results_without_report() throws Exception {
    analyzeTestProject();

    assertThat(getMeasure(PROJECT, "tests")).isNull();
    assertThat(getMeasure(PROJECT, "test_errors")).isNull();
    assertThat(getMeasure(PROJECT, "test_failures")).isNull();
    assertThat(getMeasure(PROJECT, "skipped_tests")).isNull();
  }

  @Test
  public void vstest() throws Exception {
    analyzeTestProject("sonar.vbnet.vstest.reportsPaths", "reports/vstest.trx");

    assertThat(getMeasureAsInt(PROJECT, "tests")).isEqualTo(42);
    assertThat(getMeasureAsInt(PROJECT, "test_errors")).isEqualTo(1);
    assertThat(getMeasureAsInt(PROJECT, "test_failures")).isEqualTo(10);
    assertThat(getMeasureAsInt(PROJECT, "skipped_tests")).isEqualTo(2);
  }

  @Test
  public void nunit() throws Exception {
    analyzeTestProject("sonar.vbnet.nunit.reportsPaths", "reports/nunit.xml");

    assertThat(getMeasureAsInt(PROJECT, "tests")).isEqualTo(200);
    assertThat(getMeasureAsInt(PROJECT, "test_errors")).isEqualTo(30);
    assertThat(getMeasureAsInt(PROJECT, "test_failures")).isEqualTo(20);
    assertThat(getMeasureAsInt(PROJECT, "skipped_tests")).isEqualTo(9);
  }

  @Test
  public void should_support_wildcard_patterns() throws Exception {
    analyzeTestProject("sonar.vbnet.vstest.reportsPaths", "reports/*.trx");

    assertThat(getMeasureAsInt(PROJECT, "tests")).isEqualTo(42);
  }

  private void analyzeTestProject(String... keyValues) throws IOException {
    Tests.analyzeProject(temp, PROJECT, "vbnet_no_rule", keyValues);
  }
}
