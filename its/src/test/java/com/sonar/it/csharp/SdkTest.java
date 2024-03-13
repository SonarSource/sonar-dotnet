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
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;

// Verifies that we do not fail when analyzing with various SDKs
@ExtendWith(Tests.class)
public class SdkTest {

  @TempDir
  private static Path temp;

  @Test
  void net5_should_have_metrics_and_issues() throws IOException {
    project_should_have_metrics_and_issues("Net5");
  }

  void project_should_have_metrics_and_issues(String project) throws IOException {
    Tests.analyzeProject(temp, project);
    assertThat(getComponent(project)).isNotNull();
    assertThat(getIssues(project)).extracting(Issues.Issue::getLine, Issues.Issue::getRule).containsOnly(tuple(3, "csharpsquid:S1134"));
    assertThat(getMeasureAsInt(project, "files")).isEqualTo(1);
    assertThat(getMeasureAsInt(project, "lines")).isEqualTo(5);
  }
}