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
package com.sonar.it.shared;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.Build;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.http.HttpMethod;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import com.sonar.orchestrator.locator.MavenLocation;
import com.sonar.orchestrator.util.Command;
import com.sonar.orchestrator.util.CommandExecutor;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import java.util.List;
import javax.annotation.CheckForNull;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.junit.rules.TemporaryFolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarqube.ws.Components;
import org.sonarqube.ws.Issues;
import org.sonarqube.ws.Measures;
import org.sonarqube.ws.Measures.Measure;
import org.sonarqube.ws.client.HttpConnector;
import org.sonarqube.ws.client.WsClient;
import org.sonarqube.ws.client.WsClientFactories;
import org.sonarqube.ws.client.components.ShowRequest;
import org.sonarqube.ws.client.measures.ComponentRequest;

import static java.util.Collections.singletonList;
import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;

public class TestUtils {

  final private static Logger LOG = LoggerFactory.getLogger(TestUtils.class);
  private static final String MSBUILD_PATH = "MSBUILD_PATH";

  public static Location getPluginLocation (String pluginName) {
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

  public static  ScannerForMSBuild createBeginStep(String projectName, Path projectDir) {
    return createBeginStep(projectName, projectDir, "");
  }

  public static  ScannerForMSBuild createBeginStep(String projectName, Path projectDir, String subProjectName) {
    return TestUtils.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey(projectName)
      .setProjectName(projectName)
      .setProjectVersion("1.0")
      .setProperty("sonar.projectBaseDir", getProjectBaseDir(projectDir, subProjectName));
  }

  private static String getProjectBaseDir(Path projectDir, String subProjectName){
    return projectDir.resolve(subProjectName).toString();
  }

  private static Build<ScannerForMSBuild> newScanner(Path projectDir) {
    // We need to set the fallback version to run from inside the IDE when the property isn't set
    return ScannerForMSBuild.create(projectDir.toFile())
      .setScannerVersion(System.getProperty("scannerMsbuild.version", "4.8.0.12008"))

      // In order to be able to run tests on Azure pipelines, the AGENT_BUILDDIRECTORY environment variable
      // needs to be set to the analyzed project directory.
      // This is necessary because the scanner for MsBuild will use this variable to set the correct work directory.
      .setEnvironmentVariable(VstsUtils.ENV_BUILD_DIRECTORY, projectDir.toString());
  }

  public static void runMSBuild(Orchestrator orch, Path projectDir, String... arguments) {
    Path msBuildPath = getMsBuildPath(orch);

    Command command = Command.create(msBuildPath.toString())
      .addArguments(arguments)
      .setEnvironmentVariable("AGENT_BUILDDIRECTORY", projectDir.toString())
      .setDirectory(projectDir.toFile());

    LOG.info(String.format("Running MSBuild in working directory '%s'", command.getDirectory()));

    int r = CommandExecutor.create().execute(command, 60 * 1000);
    assertThat(r).isEqualTo(0);
  }

  private static Path getMsBuildPath(Orchestrator orch) {
    String msBuildPathStr = orch.getConfiguration().getString(MSBUILD_PATH, "C:\\Program Files (x86)\\Microsoft "
      + "Visual Studio\\2017\\Enterprise\\MSBuild\\15.0\\Bin\\MSBuild.exe");
    Path msBuildPath = Paths.get(msBuildPathStr).toAbsolutePath();
    if (!Files.exists(msBuildPath)) {
      throw new IllegalStateException("Unable to find MSBuild at '" + msBuildPath.toString() +
        "'. Please configure property '" + MSBUILD_PATH + "'");
    }
    return msBuildPath;
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

  public static Components.Component getComponent(Orchestrator orch, String componentKey) {
    return newWsClient(orch).components().show(new ShowRequest().setComponent(componentKey)).getComponent();
  }

  public static List<Issues.Issue> getIssues(Orchestrator orch, String componentKey) {
    return newWsClient(orch).issues().search(new org.sonarqube.ws.client.issues.SearchRequest().setComponentKeys(Collections.singletonList(componentKey))).getIssuesList();
  }

  public static List<Hotspot> getHotspots(Orchestrator orch, String projectKey) {
    return newWsClient(orch).hotspots().search(new org.sonarqube.ws.client.hotspots.SearchRequest().setProjectKey(projectKey)).getHotspotsList();
  }

  // Versions of SonarQube and plugins support aliases:
  // - "DEV" for the latest build of master that passed QA
  // - "DEV[1.0]" for the latest build that passed QA of series 1.0.x
  // - "LATEST_RELEASE" for the latest release
  // - "LATEST_RELEASE[1.0]" for latest release of series 1.0.x
  // The SonarQube alias "LTS" has been dropped. An alternative is "LATEST_RELEASE[6.7]".
  // The term "latest" refers to the highest version number, not the most recently published version.
  public static String replaceLtsVersion(String version) {
    if (version != null && version.equals("LTS"))
    {
      return "LATEST_RELEASE[7.9]";
    }
    return version;
  }

  public static void reset(Orchestrator orchestrator) {
    LOG.info("TEST SETUP: deleting all projects...");

    orchestrator.getServer()
      .newHttpCall("/api/projects/bulk_delete")
      .setAdminCredentials()
      .setMethod(HttpMethod.POST)
      .setParams("qualifiers", "TRK")
      .execute();
  }

  static WsClient newWsClient(Orchestrator orch) {
    return WsClientFactories.getDefault().newClient(HttpConnector.newBuilder()
      .url(orch.getServer().getUrl())
      .build());
  }

  // This method has been taken from SonarSource/sonar-scanner-msbuild
  public static TemporaryFolder createTempFolder() {
    LOG.info("TEST SETUP: creating temporary folder...");

    // If the test is being run under VSTS then the Scanner will
    // expect the project to be under the VSTS sources directory
    File baseDirectory = null;
    if (VstsUtils.isRunningUnderVsts()){
      String vstsSourcePath = VstsUtils.getSourcesDirectory();
      LOG.info("TEST SETUP: Tests are running under VSTS. Build dir:  " + vstsSourcePath);
      baseDirectory = new File(vstsSourcePath);
    }
    else {
      LOG.info("TEST SETUP: Tests are not running under VSTS");
    }

    TemporaryFolder folder = new TemporaryFolder(baseDirectory);
    LOG.info("TEST SETUP: Temporary folder created. Base directory: " + baseDirectory);
    return folder;
  }

  public static void deleteLocalCache() {
    // Scanner for MSBuild caches the analyzer, so running the test twice in a row means the old binary is used.
    String localAppData = System.getenv("LOCALAPPDATA") + "\\Temp\\.sonarqube";
    try {
      FileUtils.deleteDirectory(new File(localAppData));
    }
    catch (IOException ioe) {
      throw new IllegalStateException("could not delete Scanner for MSBuild cache folder", ioe);
    }
  }

}
