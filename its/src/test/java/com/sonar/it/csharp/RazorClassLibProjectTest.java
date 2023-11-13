/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2023 SonarSource SA
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

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import java.nio.file.Path;
import java.util.List;
import java.util.stream.Collectors;

import static com.sonar.it.csharp.Tests.getComponent;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class RazorClassLibProjectTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT = "RazorClassLib";
  private static final String RAZOR_COMPONENT_CLASS_FILE = "RazorClassLib:Component.razor";
  private static final String S6798_FOLDER = "RazorClassLib:S6798";
  private static final String S6800_FOLDER = "RazorClassLib:S6800";

  @BeforeAll
  public static void beforeAll() throws Exception {
    Tests.analyzeProject(PROJECT, temp, PROJECT);
  }

  @Test
  void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo("RazorClassLib");

    assertThat(getComponent(RAZOR_COMPONENT_CLASS_FILE).getName()).isEqualTo("Component.razor");
  }

  @Test
  void issuesOfS6798AreRaised() {
    var issues = Tests.getIssues(PROJECT).stream().filter(x -> x.getRule().startsWith("csharpsquid:S6798")).collect(Collectors.toList());

    assertThat(issues).hasSize(4);
    assertThat(issues.stream().filter(x -> x.getComponent().equals(S6798_FOLDER + "/S6798.CSharpOnly.cs"))).hasSize(1);
    assertThat(issues.stream().filter(x -> x.getComponent().equals(S6798_FOLDER + "/S6798.Partial.razor"))).hasSize(1);
    assertThat(issues.stream().filter(x -> x.getComponent().equals(S6798_FOLDER + "/S6798.Partial.razor.cs"))).hasSize(1);
    assertThat(issues.stream().filter(x -> x.getComponent().equals(S6798_FOLDER + "/S6798.razor"))).hasSize(1);
  }

  @Test
  void issuesOfS6800AreRaised() {
    var issues = Tests.getIssues(PROJECT).stream().filter(x -> x.getRule().startsWith("csharpsquid:S6800")).collect(Collectors.toList());

    assertThat(issues).hasSize(3);
    assertThat(issues.stream().filter(issue -> issue.getComponent().equals(S6800_FOLDER + "/S6800.CsharpOnly.cs"))).hasSize(1);
    assertThat(issues.stream().filter(issue -> issue.getComponent().equals(S6800_FOLDER + "/S6800.razor"))).hasSize(1);
    assertThat(issues.stream().filter(issue -> issue.getComponent().equals(S6800_FOLDER + "/S6800.Partial.razor.cs"))).hasSize(1);
  }
}
