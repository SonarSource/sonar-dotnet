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

import java.io.IOException;
import java.nio.file.Path;
import java.util.List;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class CognitiveComplexityTest {
  private static final String LANGUAGE_KEY = "cs";
  private static final String PROFILE_NAME = "custom_complexity";
  private static final String PROJECT_NAME = "CognitiveComplexity.CS";

  @TempDir
  private static Path temp;

  @BeforeAll
  public static void init() throws IOException {
    provisionProject();
    Tests.analyzeProject(temp, PROJECT_NAME);
  }

  @Test
  void cognitiveComplexity_hasSecondaryLocations() {

    final String componentKey = "CognitiveComplexity.CS:CognitiveComplexity.cs";

    assertThat(getComponent(componentKey)).isNotNull();
    List<Issues.Issue> issues = getIssues(componentKey);

    assertThat(issues.size()).isEqualTo(1);
    assertThat(issues.get(0).getFlowsCount()).isEqualTo(3);
    assertThat(issues.get(0).getFlows(0).getLocations(0).getTextRange().getStartLine()).isEqualTo(3);
    assertThat(issues.get(0).getFlows(1).getLocations(0).getTextRange().getStartLine()).isEqualTo(7);
    assertThat(issues.get(0).getFlows(2).getLocations(0).getTextRange().getStartLine()).isEqualTo(11);
  }

  private static void provisionProject() {
    ORCHESTRATOR.getServer().provisionProject(PROJECT_NAME, PROJECT_NAME);
    ORCHESTRATOR.getServer().associateProjectToQualityProfile(PROJECT_NAME, LANGUAGE_KEY, PROFILE_NAME);
  }
}
