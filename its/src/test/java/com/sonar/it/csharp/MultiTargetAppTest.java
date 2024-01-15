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
import java.util.stream.Collectors;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class MultiTargetAppTest {

  @TempDir
  private static Path temp;

  @Test
  void should_analyze_multitarget_project() throws IOException {
    String componentKey = "MultiTargetConsoleApp:MultiTargetConsoleApp/Program.cs";
    var buildResult = Tests.analyzeProject(temp, "MultiTargetConsoleApp");
    assertThat(buildResult.getLogs()).contains("Found 1 MSBuild C# project: 1 MAIN project.");
    assertThat(getComponent(componentKey)).isNotNull();
    assertThat(getIssues(componentKey).stream().filter(x -> x.getRule().startsWith("csharpsquid:")).collect(Collectors.toList())).hasSize(4);
  }
}
