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

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import java.nio.file.Path;
import java.util.List;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class NoSourcesTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT = "ProjectWithNoSources";
  private static BuildResult buildResult;

  @BeforeAll
  public static void init() throws Exception {
    buildResult = Tests.analyzeProject(temp, PROJECT);
  }

  @Test
  void raiseOnSharedFiles() {
    List<Issues.Issue> issues = TestUtils.getIssues(ORCHESTRATOR, PROJECT);
    assertThat(issues).hasSize(1).hasOnlyOneElementSatisfying(e ->
    {
      assertThat(e.getLine()).isEqualTo(5);
      assertThat(e.getRule()).isEqualTo("csharpsquid:S1186");
    });
  }

  @Test
  void logsContainInfo() {
    assertThat(buildResult.getLogs()).contains("Found 1 MSBuild C# project: 1 with no MAIN nor TEST files.");
  }
}
