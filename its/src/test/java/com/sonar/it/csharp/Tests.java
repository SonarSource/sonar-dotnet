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

import com.sonar.it.shared.OrchestratorState;
import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.locator.FileLocation;
import java.io.IOException;
import java.nio.file.Path;
import java.util.List;
import javax.annotation.CheckForNull;
import javax.annotation.Nullable;
import com.sonar.orchestrator.locator.MavenLocation;
import org.junit.jupiter.api.extension.AfterAllCallback;
import org.junit.jupiter.api.extension.BeforeAllCallback;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.sonarqube.ws.Components;
import org.sonarqube.ws.Issues;
import org.sonarqube.ws.Measures;

import static org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;

public class Tests implements BeforeAllCallback, AfterAllCallback {

  public static final Orchestrator ORCHESTRATOR = TestUtils.prepareOrchestrator()
    .addPlugin(TestUtils.getPluginLocation("sonar-csharp-plugin")) // Do not add VB.NET here, use shared project instead
    // The HtmlPlugin is needed for Razor related integration tests
    // We want to rely on HtmlPlugin to be able to report cshtml issues
    // otherwise .cshtml file are not pushed to SQ
    .addPlugin(MavenLocation.of("org.sonarsource.html", "sonar-html-plugin", "LATEST_RELEASE"))
    .restoreProfileAtStartup(FileLocation.of("profiles/no_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/blazor_rules.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/class_name.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/template_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_parameters.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_complexity.xml"))
    .build();

  private static final OrchestratorState ORCHESTRATOR_STATE = new OrchestratorState(ORCHESTRATOR);

  @Override
  public void beforeAll(ExtensionContext extensionContext) throws Exception {
    ORCHESTRATOR_STATE.startOnce();
  }

  @Override
  public void afterAll(ExtensionContext extensionContext) throws Exception {
    ORCHESTRATOR_STATE.stopOnce();
  }

  public static BuildResult analyzeProject(Path temp, String projectDir) throws IOException {
    return analyzeProject(projectDir, temp, projectDir);
  }

  public static BuildResult analyzeProject(String projectKey, Path temp, String projectDir) throws IOException {
    return analyzeProject(projectKey, temp, projectDir, null);
  }

  public static BuildResult analyzeProject(Path temp, String projectDir, @Nullable String profileKey, String... keyValues) throws IOException {
    return analyzeProject(projectDir, temp, projectDir, profileKey, keyValues);
  }

  public static BuildResult analyzeProject(String projectKey, Path temp, String projectDir, @Nullable String profileKey, String... keyValues) throws IOException {
    Path projectFullPath = TestUtils.projectDir(temp, projectDir);

    return analyzeProjectPath(projectKey, projectFullPath, profileKey, keyValues);
  }

  public static BuildResult analyzeProjectPath(String projectKey, Path projectFullPath, @Nullable String profileKey, String... keyValues) {
    ScannerForMSBuild beginStep = TestUtils.createBeginStep(projectKey, projectFullPath)
      .setProfile(profileKey)
      .setProperties(keyValues);

    ORCHESTRATOR.executeBuild(beginStep);
    TestUtils.runBuild(projectFullPath);
    return ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectFullPath));
  }

  public static Components.Component getComponent(String componentKey) {
    return TestUtils.getComponent(ORCHESTRATOR, componentKey);
  }

  @CheckForNull
  public static Integer getMeasureAsInt(String componentKey, String metricKey) {
    return TestUtils.getMeasureAsInt(ORCHESTRATOR, componentKey, metricKey);
  }

  @CheckForNull
  public static Measures.Measure getMeasure(String componentKey, String metricKey) {
    return TestUtils.getMeasure(ORCHESTRATOR, componentKey, metricKey);
  }

  public static List<Issues.Issue> getIssues(String componentKey) {
    return TestUtils.getIssues(ORCHESTRATOR, componentKey);
  }

  public static List<Hotspot> getHotspots(String projectKey) {
    return TestUtils.getHotspots(ORCHESTRATOR, projectKey);
  }
}
