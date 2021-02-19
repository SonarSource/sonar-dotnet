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
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues;

import java.io.IOException;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;

public class UnitTestProjectTypeProbingTest {
  private static final String PROJECT = "UTProjectProbing";
  private static final String MAIN_RULE_ID = "csharpsquid:S1048";
  private static final String MAIN_AND_TEST_RULE_ID = "csharpsquid:S101";
  private static Boolean isProjectAnalyzed = false;

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() throws IOException {
    if (!isProjectAnalyzed) {
      TestUtils.reset(ORCHESTRATOR);
      Tests.analyzeProject(temp, PROJECT, null);

      isProjectAnalyzed = true;
    }
  }

  @Test
  public void mainProject_IsIdentifiedAsMain() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.Main/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(MAIN_RULE_ID, MAIN_AND_TEST_RULE_ID);
  }

  @Test
  public void mainProject_WithPropertySetToTrue_IsIdentifiedAsTestProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.MainWithProjectPropertyTrue/calculator.cs")).isEmpty();
  }

  @Test
  public void testProject_WithPropertySetToFalse_IsIdentifiedAsMainProjectByScannerAndTestByAnalyzer() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.MsTestWithProjectPropertyFalse/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactly(MAIN_AND_TEST_RULE_ID);
  }

  @Test
  public void msTestProject_IsIdentifiedAsTestProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.MsTest/calculator.cs")).isEmpty();
  }

  @Test
  public void xUnitProject_IsIdentifiedAsTestProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.xUnit/calculator.cs")).isEmpty();
  }

  @Test
  public void project_WithTestInName_IsIdentifiedAsMainProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.ContainsTestInName/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(MAIN_RULE_ID, MAIN_AND_TEST_RULE_ID);
  }

  @Test
  public void project_WithTestsSuffix_IsIdentifiedAsTestProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.EndsWithTests/calculator.cs")).isEmpty();
  }
}
