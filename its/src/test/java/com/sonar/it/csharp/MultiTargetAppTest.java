/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2017 SonarSource SA
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

import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues;

import com.sonar.orchestrator.Orchestrator;

import java.nio.file.Path;
import java.util.List;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class MultiTargetAppTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void init() throws Exception {
    orchestrator.resetData();
  }

  @Test
  public void should_analyze_multitarget_project() throws Exception {
    Path projectDir = Tests.projectDir(temp, "MultiTargetConsoleApp");
    orchestrator.executeBuild(Tests.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("MultiTargetConsoleApp")
      .setProjectName("MultiTargetConsoleApp")
      .setProjectVersion("1.0"));

    Tests.runNuGet(orchestrator, projectDir, "restore");
    Tests.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(Tests.newScanner(projectDir)
      .addArgument("end"));

    assertThat(getComponent("MultiTargetConsoleApp:Program.cs")).isNotNull();
    //assertThat(getComponent("MultiTargetConsoleApp:MultiTargetConsoleApp:FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"+ ":Program.cs")).isNotNull();

    //assertThat(getMeasureAsInt("MultiTargetConsoleApp:Class1.cs", "files")).isEqualTo(1);
    //assertThat(getMeasureAsInt("MultiTargetConsoleApp:Class1.cs", "lines")).isEqualTo(9);
    //assertThat(getMeasureAsInt("MultiTargetConsoleApp:Class1.cs", "ncloc")).isEqualTo(7);

    //List<Issues.Issue> issues = getIssues("MultiTargetConsoleApp:Program.cs");
    //assertThat(issues).hasSize(1);
  }
}
