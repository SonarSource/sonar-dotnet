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
package com.sonar.it.shared;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.Build;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.container.Edition;
import com.sonar.orchestrator.junit5.OrchestratorExtension;
import com.sonar.orchestrator.junit5.OrchestratorExtensionBuilder;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import com.sonar.orchestrator.locator.MavenLocation;
import com.sonar.orchestrator.util.Command;
import com.sonar.orchestrator.util.CommandExecutor;
import java.io.File;
import java.io.IOException;
import java.io.StringWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;
import javax.annotation.CheckForNull;

import com.sonar.orchestrator.util.StreamConsumer;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarqube.ws.Ce;
import org.sonarqube.ws.Components;
import org.sonarqube.ws.Duplications.ShowResponse;
import org.sonarqube.ws.Issues;
import org.sonarqube.ws.Measures;
import org.sonarqube.ws.Measures.Measure;
import org.sonarqube.ws.client.HttpConnector;
import org.sonarqube.ws.client.WsClient;
import org.sonarqube.ws.client.WsClientFactories;
import org.sonarqube.ws.client.ce.TaskRequest;
import org.sonarqube.ws.client.components.ShowRequest;
import org.sonarqube.ws.client.measures.ComponentRequest;

import static java.util.Collections.singletonList;
import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;

public class TestUtils {

  final private static String NUGET_CONFIG_FILE_NAME = "NuGet.config";

  final private static Logger LOG = LoggerFactory.getLogger(TestUtils.class);

  public static OrchestratorExtensionBuilder prepareOrchestrator() {
    return OrchestratorExtension.builderEnv()
      .useDefaultAdminCredentialsForBuilds(true)
      // See https://github.com/SonarSource/orchestrator#version-aliases
      .setSonarVersion(System.getProperty("sonar.runtimeVersion", "DEV"))
      .setEdition(Edition.DEVELOPER)
      .activateLicense();
  }

  public static Path projectDir(Path temp, String projectName) throws IOException {
    Path projectDir = Paths.get("projects").resolve(projectName);
    Path newFolder = temp.resolve(projectName);
    FileUtils.deleteDirectory(newFolder.toFile());
    Files.createDirectory(newFolder);
    FileUtils.copyDirectory(projectDir.toFile(), newFolder.toFile());
    FileUtils.copyFile(Paths.get("projects", NUGET_CONFIG_FILE_NAME).toFile(), Paths.get(newFolder.toString(), NUGET_CONFIG_FILE_NAME).toFile());
    return newFolder.toRealPath();
  }

  // Ensure no AnalysisWarning is raised inside the SQ GUI
  public static void verifyNoGuiWarnings(Orchestrator orchestrator, BuildResult buildResult) {
    Ce.Task task = TestUtils.getAnalysisWarningsTask(orchestrator, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    assertThat(task.getWarningsList()).isEmpty();
  }

  public static void verifyGuiTestOnlyProjectAnalysisWarning(Orchestrator orchestrator, BuildResult buildResult, String language)
  {
    verifyGuiTestOnlyProjectAnalysisWarning(orchestrator, buildResult, language, new String[0]);
  }

  // Verify an AnalysisWarning is raised inside the SQ GUI (on the project dashboard)
  public static void verifyGuiTestOnlyProjectAnalysisWarning(Orchestrator orchestrator, BuildResult buildResult, String language, String... additionalWarnings) {
    Ce.Task task = TestUtils.getAnalysisWarningsTask(orchestrator, buildResult);
    assertThat(task.getStatus()).isEqualTo(Ce.TaskStatus.SUCCESS);
    List<String> expectedWarnings = new ArrayList<>();
    expectedWarnings.add("Your project contains only TEST-code for language " + language
      + " and no MAIN-code for any language, so only TEST-code related results are imported. "
      + "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. "
      + "Read more about how the SonarScanner for .NET detects test projects: "
      + "https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects");
    Collections.addAll(expectedWarnings, additionalWarnings);
    assertThat(task.getWarningsList()).containsExactlyElementsOf(expectedWarnings);
  }

  public static Ce.Task getAnalysisWarningsTask(Orchestrator orchestrator, BuildResult buildResult) {
    String taskId = extractCeTaskId(buildResult);
    return newWsClient(orchestrator)
      .ce()
      .task(new TaskRequest().setId(taskId).setAdditionalFields(Collections.singletonList("warnings")))
      .getTask();
  }

  public static Location getPluginLocation(String pluginName) {
    Location pluginLocation;

    String version = System.getProperty("csharpVersion"); // C# and VB.Net versions are the same
    LOG.info("Locating plugin: " + pluginName + " Version: " + version);

    if (StringUtils.isEmpty(version)) {
      // use the plugin that was built on local machine
      LOG.info("Using local plugin version");
      pluginLocation = FileLocation.byWildcardMavenFilename(new File("../" + pluginName + "/target"), pluginName + "-*.jar");
    } else {
      // QA environment downloads the plugin built by the CI job
      LOG.info("Using version from Maven");
      pluginLocation = MavenLocation.of("org.sonarsource.dotnet", pluginName, version);
    }

    LOG.info("Plugin location=" + pluginLocation);
    return pluginLocation;
  }

  public static ScannerForMSBuild createEndStep(Path projectDir) {
    return TestUtils.newScanner(projectDir)
      .addArgument("end");
  }

  public static ScannerForMSBuild createBeginStep(String projectName, Path projectDir) {
    return createBeginStep(projectName, projectDir, "");
  }

  public static ScannerForMSBuild createBeginStep(String projectName, Path projectDir, String subProjectName) {
    return TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey(projectName)
      .setProjectName(projectName)
      .setProjectVersion("1.0")
      .setProperty("sonar.projectBaseDir", getProjectBaseDir(projectDir, subProjectName));
  }

  private static String getProjectBaseDir(Path projectDir, String subProjectName) {
    return projectDir.resolve(subProjectName).toString();
  }

  private static Build<ScannerForMSBuild> newScanner(Path projectDir) {
    // We need to set the fallback version to run from inside the IDE when the property isn't set
    return ScannerForMSBuild.create(projectDir.toFile())
      .setScannerVersion(ScannerForMSBuild.LATEST_RELEASE)

      // In order to be able to run tests on Azure pipelines, the AGENT_BUILDDIRECTORY environment variable
      // needs to be set to the analyzed project directory.
      // This is necessary because the SonarScanner for .NET will use this variable to set the correct work directory.
      .setEnvironmentVariable(VstsUtils.ENV_BUILD_DIRECTORY, projectDir.toString());
  }

  public static void runBuild(Path projectDir, String... arguments) {
    List<String> argumentList = new ArrayList<>(Arrays.asList(arguments));
    argumentList.add(0, "build");
    argumentList.add("/warnaserror:AD0001;CS8032;BC42376");
    // This is mandatory otherwise process node locks dlls in .sonarqube preventing the test to delete temp directory
    argumentList.add("/nr:false");

    Command command = Command.create("dotnet")
      .addArguments(argumentList)
      .setEnvironmentVariable("AGENT_BUILDDIRECTORY", projectDir.toString())
      .setDirectory(projectDir.toFile());

    LOG.info(String.format("Running `dotnet build` in working directory '%s'", command.getDirectory()));
    var logWriter = new StringWriter();
    StreamConsumer.Pipe logsConsumer = new StreamConsumer.Pipe(logWriter);
    int buildResult = CommandExecutor.create().execute(command, logsConsumer, 2 * 60 * 1000);
    LOG.info(logWriter.toString());
    assertThat(buildResult).isZero();
  }

  @CheckForNull
  public static Measure getMeasure(Orchestrator orch, String componentKey, String metricKey) {
    Measures.ComponentWsResponse response = newWsClient(orch).measures().component(new ComponentRequest()
      .setComponent(componentKey)
      .setMetricKeys(singletonList(metricKey)));
    List<Measure> measures = response.getComponent().getMeasuresList();
    return measures.size() == 1 ? measures.get(0) : null;
  }

  @CheckForNull
  public static Integer getMeasureAsInt(Orchestrator orch, String componentKey, String metricKey) {
    Measure measure = getMeasure(orch, componentKey, metricKey);
    return (measure == null) ? null : Integer.parseInt(measure.getValue());
  }

  @CheckForNull
  public static ShowResponse getDuplication(Orchestrator orch, String componentKey) {
    ShowResponse response = newWsClient(orch).duplications().show(new org.sonarqube.ws.client.duplications.ShowRequest()
      .setKey(componentKey));
    return response;
  }

  @CheckForNull
  public static ShowResponse getDuplication(Orchestrator orch, String componentKey, String pullRequestKey) {
    ShowResponse response = newWsClient(orch).duplications().show(new org.sonarqube.ws.client.duplications.ShowRequest()
      .setKey(componentKey)
      .setPullRequest(pullRequestKey));
    return response;
  }

  public static Components.Component getComponent(Orchestrator orch, String componentKey) {
    return newWsClient(orch).components().show(new ShowRequest().setComponent(componentKey)).getComponent();
  }

  public static List<Issues.Issue> getIssues(Orchestrator orch, String componentKey) {
    return newWsClient(orch).issues().search(new org.sonarqube.ws.client.issues.SearchRequest().setComponentKeys(Collections.singletonList(componentKey))).getIssuesList();
  }

  public static List<Issues.Issue> getIssues(Orchestrator orch, String componentKey, String pullRequestKey) {
    return newWsClient(orch).issues()
      .search(new org.sonarqube.ws.client.issues.SearchRequest().setComponentKeys(Collections.singletonList(componentKey)).setPullRequest(pullRequestKey)).getIssuesList();
  }

  public static List<Hotspot> getHotspots(Orchestrator orch, String projectKey) {
    return newWsClient(orch).hotspots().search(new org.sonarqube.ws.client.hotspots.SearchRequest().setProjectKey(projectKey)).getHotspotsList();
  }

  private static WsClient newWsClient(Orchestrator orch) {
    return WsClientFactories.getDefault().newClient(HttpConnector.newBuilder()
      .url(orch.getServer().getUrl())
      .token(orch.getDefaultAdminToken())
      .build());
  }

  public static void deleteLocalCache() {
    // SonarScanner for .NET caches the analyzer, so running the test twice in a row means the old binary is used.
    File file = new File(System.getenv("LOCALAPPDATA") + "\\Temp\\.sonarqube");
    LOG.info("TEST SETUP: deleting local analyzers cache: " + file.toString());
    try {
      if (file.exists()) {
        FileUtils.deleteDirectory(file);
      }
    } catch (IOException ioe) {
      throw new IllegalStateException("Could not delete SonarScanner for .NET cache folder", ioe);
    }
  }

  private static String extractCeTaskId(BuildResult buildResult) {
    List<String> taskIds = extractCeTaskIds(buildResult);
    if (taskIds.size() != 1) {
      throw new IllegalStateException("More than one task id retrieved from logs.");
    }
    return taskIds.iterator().next();
  }

  private static List<String> extractCeTaskIds(BuildResult buildResult) {
    // The log looks like this:
    // INFO: More about the report processing at http://127.0.0.1:53395/api/ce/task?id=0f639b4c-6421-4620-81d0-eac0f5759f06
    return buildResult.getLogsLines(s -> s.contains("More about the report processing at")).stream()
      .map(s -> s.substring(s.lastIndexOf("=") + 1))
      .collect(Collectors.toList());
  }
}
