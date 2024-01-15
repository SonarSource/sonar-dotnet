/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2024 SonarSource SA
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

import com.sonar.orchestrator.build.BuildResult;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class UnitTestProjectTypeProbingTest {
  private static final String PROJECT = "UTProjectProbing";
  private static final String MAIN_RULE_ID = "csharpsquid:S1048";
  private static final String MAIN_AND_TEST_RULE_ID = "csharpsquid:S101";
  private static final String REMOVE_EMPTY_FINALIZERS = "external_roslyn:CA1821";

  private static BuildResult buildResult;

  @TempDir
  private static Path temp;

  @BeforeAll
  public static void init() throws IOException {
    buildResult = Tests.analyzeProject(temp, PROJECT);
  }

  @Test
  void mainProject_IsIdentifiedAsMain() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.Main/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(MAIN_RULE_ID, MAIN_AND_TEST_RULE_ID, REMOVE_EMPTY_FINALIZERS);
  }

  @Test
  void mainProject_WithPropertySetToTrue_IsIdentifiedAsTestProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.MainWithProjectPropertyTrue/Calculator.cs")).isEmpty();
  }

  @Test
  void testProject_WithPropertySetToFalse_IsIdentifiedAsMainProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.MsTestWithProjectPropertyFalse/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(MAIN_AND_TEST_RULE_ID, MAIN_RULE_ID, REMOVE_EMPTY_FINALIZERS);
  }

  @Test
  void msTestProject_IsIdentifiedAsMainProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.MsTest/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactly(REMOVE_EMPTY_FINALIZERS);
  }

  @Test
  void xUnitProject_IsIdentifiedAsTestProject() {
    // project has the ProjectCapability 'TestContainer' -> test project
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.xUnit/Calculator.cs")).isEmpty();
  }

  @Test
  void project_WithTestInName_IsIdentifiedAsMainProject() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.ContainsTestInName/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(MAIN_RULE_ID, MAIN_AND_TEST_RULE_ID, REMOVE_EMPTY_FINALIZERS);
  }

  @Test
  void project_WithTestsSuffix_IsIdentifiedAsMain() {
    assertThat(getIssues("UTProjectProbing:UTProjectProbing.EndsWithTests/calculator.cs"))
      .extracting(Issues.Issue::getRule)
      .containsExactlyInAnyOrder(MAIN_RULE_ID, MAIN_AND_TEST_RULE_ID, REMOVE_EMPTY_FINALIZERS);
  }

  @Test
  void logsContainInfo() {
    assertThat(buildResult.getLogs()).contains("Found 7 MSBuild C# projects: 4 MAIN projects. 3 TEST projects.");
  }
}
