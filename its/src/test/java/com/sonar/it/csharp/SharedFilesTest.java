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
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import java.nio.file.Path;
import java.util.List;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues.Issue;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class SharedFilesTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void init() {
    orchestrator.resetData();
  }
  
  @Test
  public void should_analyze_shared_files() throws Exception {
    Path projectDir = Tests.projectDir(temp, "SharedFilesTest");
    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("SharedFilesTest")
      .setProjectVersion("1.0")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    TestUtils.runNuGet(orchestrator, projectDir, "restore");
    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));

    assertThat(getComponent("SharedFilesTest:Class1.cs")).isNotNull();
    assertThat(getComponent(TestUtils.hasModules(ORCHESTRATOR) ? "SharedFilesTest:SharedFilesTest:77C8C6B5-18EC-45D4-8DA8-17A6525450A4:Program1.cs" : "SharedFilesTest:ConsoleApp1/Program1.cs")).isNotNull();
    assertThat(getComponent(TestUtils.hasModules(ORCHESTRATOR) ? "SharedFilesTest:SharedFilesTest:0FAF9365-FC72-4DF6-A466-7C432E85F2A8:Program.cs" : "SharedFilesTest:ConsoleApp2/Program.cs")).isNotNull();

    // shared file in the solution should have measures and issues
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "files")).isEqualTo(1);
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "lines")).isEqualTo(9);
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "ncloc")).isEqualTo(7);
    
    List<Issue> issues = getIssues("SharedFilesTest:Class1.cs");
    assertThat(issues).hasSize(1);
  }
}
