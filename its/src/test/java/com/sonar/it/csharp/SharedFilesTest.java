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
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.nio.file.Path;
import java.util.List;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues.Issue;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class SharedFilesTest {
  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void init() {
    TestUtils.reset(orchestrator);
  }
  
  @Test
  public void should_analyze_shared_files() throws Exception {
    Path projectDir = Tests.projectDir(temp, "SharedFilesTest");

    ScannerForMSBuild beginStep = TestUtils.createBeginStep("SharedFilesTest", projectDir)
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml");

    orchestrator.executeBuild(beginStep);

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Restore", "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.createEndStep(projectDir));

    assertThat(getComponent("SharedFilesTest:Class1.cs")).isNotNull();
    assertThat(getComponent("SharedFilesTest:ConsoleApp1/Program1.cs")).isNotNull();
    assertThat(getComponent("SharedFilesTest:ConsoleApp2/Program.cs")).isNotNull();

    // shared file in the solution should have measures and issues
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "files")).isEqualTo(1);
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "lines")).isEqualTo(7);
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "ncloc")).isEqualTo(6);
    
    List<Issue> issues = getIssues("SharedFilesTest:Class1.cs");
    assertThat(issues).hasSize(1);
  }
}
