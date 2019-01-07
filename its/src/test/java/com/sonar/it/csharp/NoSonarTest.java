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
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class NoSonarTest {

  @ClassRule
  public static TemporaryFolder temp = new TemporaryFolder();

  private static final String PROJECT = "NoSonarTest";
  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @BeforeClass
  public static void init() throws Exception {
    orchestrator.resetData();

    Path projectDir = Tests.projectDir(temp, "NoSonarTest");
    ORCHESTRATOR.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("NoSonarTest")
      .setProjectName("NoSonarTest")
      .setProjectVersion("1.0")
      .setProfile("class_name")
      // Without that, the NoSonarTest project is considered as a Test project :)
      .setProperty("sonar.msbuild.testProjectPattern", "noTests"));

    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Rebuild");

    ORCHESTRATOR.executeBuild(TestUtils.newScanner(projectDir)
      .addArgument("end"));
  }

  @Test
  public void filesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("violations")).isEqualTo(1);
  }

  /* Helper methods */

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT, metricKey);
  }

}
