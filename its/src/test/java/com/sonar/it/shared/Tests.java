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
package com.sonar.it.shared;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.locator.MavenLocation;
import java.io.IOException;
import java.nio.file.Path;
import javax.annotation.CheckForNull;
import javax.annotation.Nullable;
import org.junit.ClassRule;
import org.junit.runner.RunWith;
import org.junit.runners.Suite;
import org.junit.runners.Suite.SuiteClasses;

@RunWith(Suite.class)
@SuiteClasses({
  CoverageTest.class,
  EnsureAllTestsRunTest.class,
  ScannerCliTest.class,
  VbMainCodeCsTestCodeTest.class
})

public class Tests {

  @ClassRule
  public static final Orchestrator ORCHESTRATOR = TestUtils.prepareOrchestrator()
    .addPlugin(TestUtils.getPluginLocation("sonar-csharp-plugin"))
    .addPlugin(TestUtils.getPluginLocation("sonar-vbnet-plugin"))
    // ScannerCliTest: Fixed version for the HTML plugin as we don't want to have failures in case of changes there.
    .addPlugin(MavenLocation.of("org.sonarsource.html", "sonar-html-plugin", "3.2.0.2082"))
    .build();

  static BuildResult analyzeProject(Path temp, String projectDir) throws IOException {
    return analyzeProject(temp, projectDir, null);
  }

  static BuildResult analyzeProject(Path temp, String projectDir, @Nullable String profileKey, String... keyValues) throws IOException {
    Path projectFullPath = TestUtils.projectDir(temp, projectDir);
    ScannerForMSBuild beginStep = TestUtils.createBeginStep(projectDir, projectFullPath)
      .setProfile(profileKey)
      .setProperties(keyValues);
    ORCHESTRATOR.executeBuild(beginStep);
    TestUtils.runMSBuild(ORCHESTRATOR, projectFullPath, "/t:Restore,Rebuild");
    return ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectFullPath));
  }

  @CheckForNull
  static Integer getMeasureAsInt(String componentKey, String metricKey) {
    return TestUtils.getMeasureAsInt(ORCHESTRATOR, componentKey, metricKey);
  }
}
