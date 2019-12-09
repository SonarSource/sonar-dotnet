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

import java.io.File;
import java.util.stream.Collectors;

import com.sonar.orchestrator.build.ScannerForMSBuild;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues;

import com.sonar.orchestrator.Orchestrator;

import java.nio.file.Path;
import java.util.List;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;

public class MultiTargetAppTest {
  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void init() {
    orchestrator.resetData();
  }

  @Test
  public void should_analyze_multitarget_project() throws Exception {
    Path projectDir = Tests.projectDir(temp, "MultiTargetConsoleApp");

    ScannerForMSBuild beginStep = TestUtils.createStartStep("MultiTargetConsoleApp", projectDir, "MultiTargetConsoleApp");

    orchestrator.executeBuild(beginStep);

    TestUtils.runMSBuild(orchestrator, projectDir, "/t:Restore", "/t:Rebuild");

    orchestrator.executeBuild(TestUtils.newEndStep(projectDir));

    String programCsComponentId = TestUtils.hasModules(ORCHESTRATOR) ? "MultiTargetConsoleApp:MultiTargetConsoleApp:9D7FB932-3B1E-446D-9D34-A63410458B88:Program.cs" : "MultiTargetConsoleApp:Program.cs";
    assertThat(getComponent(programCsComponentId)).isNotNull();

    List<Issues.Issue> issues = getIssues(programCsComponentId)
      .stream()
      .filter(x -> x.getRule().startsWith("csharpsquid:"))
      .collect(Collectors.toList());
    assertThat(issues).hasSize(4);
  }
}
