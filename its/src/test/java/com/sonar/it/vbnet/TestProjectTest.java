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
package com.sonar.it.vbnet;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import java.nio.file.Path;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static com.sonar.it.vbnet.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class TestProjectTest {

  @TempDir
  private static Path temp;

  private static BuildResult buildResult;

  @BeforeAll
  public static void init() throws Exception {
    buildResult = Tests.analyzeProject(temp, "VbTestOnlyProjectTest");
  }

  @Test
  public void with_vbnet_only_test_should_not_populate_metrics() throws Exception {
    assertThat(Tests.getComponent("VbTestOnlyProjectTest:MyLib.Tests/UnitTest1.vb")).isNotNull();
    assertThat(getMeasureAsInt("VbTestOnlyProjectTest", "files")).isNull();
    assertThat(getMeasureAsInt("VbTestOnlyProjectTest", "lines")).isNull();
    assertThat(getMeasureAsInt("VbTestOnlyProjectTest", "ncloc")).isNull();

    assertThat(buildResult.getLogsLines(l -> l.contains("WARN")))
      .contains("WARN: SonarScanner for .NET detected only TEST files and no MAIN files for VB.NET in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " +
        "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");

    assertThat(buildResult.getLogsLines(l -> l.contains("INFO"))).contains("INFO: Found 1 MSBuild VB.NET project: 1 TEST project.");
    TestUtils.verifyGuiTestOnlyProjectAnalysisWarning(ORCHESTRATOR, buildResult, "VB.NET");
  }

  @Test
  public void roslynVersionIsLogged() {
    assertThat(buildResult.getLogsLines(x -> x.startsWith("INFO: Roslyn version: "))).hasSize(1);
  }
}
