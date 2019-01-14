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
package com.sonar.it.shared;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.Build;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import com.sonar.orchestrator.locator.MavenLocation;
import com.sonar.orchestrator.util.Command;
import com.sonar.orchestrator.util.CommandExecutor;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import java.util.List;
import javax.annotation.CheckForNull;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import org.sonarqube.ws.Issues;
import org.sonarqube.ws.WsComponents;
import org.sonarqube.ws.WsMeasures;
import org.sonarqube.ws.WsMeasures.Measure;
import org.sonarqube.ws.client.HttpConnector;
import org.sonarqube.ws.client.WsClient;
import org.sonarqube.ws.client.WsClientFactories;
import org.sonarqube.ws.client.component.ShowWsRequest;
import org.sonarqube.ws.client.issue.SearchWsRequest;
import org.sonarqube.ws.client.measure.ComponentWsRequest;

import static java.util.Collections.singletonList;
import static org.assertj.core.api.Assertions.assertThat;

public class TestUtils {

  final private static Logger LOG = LoggerFactory.getLogger(TestUtils.class);
  private static final String MSBUILD_PATH = "MSBUILD_PATH";
  private static final String NUGET_PATH = "NUGET_PATH";

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

  public static Build<ScannerForMSBuild> newScanner(Path projectDir) {
    return ScannerForMSBuild.create(projectDir.toFile())
      .setScannerVersion(System.getProperty("scannerMsbuild.version"));
  }

  public static void runMSBuild(Orchestrator orch, Path projectDir, String... arguments) {
    Path msBuildPath = getMsBuildPath(orch);

    int r = CommandExecutor.create().execute(Command.create(msBuildPath.toString())
      .addArguments(arguments)
      .setDirectory(projectDir.toFile()), 60 * 1000);
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

  public static void runNuGet(Orchestrator orch, Path projectDir, String... arguments) {
    Path nugetPath = getNuGetPath(orch);

    int r = CommandExecutor.create().execute(Command.create(nugetPath.toString())
      .addArguments(arguments)
      .setDirectory(projectDir.toFile()), 60 * 1000);
    assertThat(r).isEqualTo(0);
  }

  private static Path getNuGetPath(Orchestrator orch) {
    String nugetPathStr = orch.getConfiguration().getString(NUGET_PATH);
    Path nugetPath = Paths.get(nugetPathStr).toAbsolutePath();
    if (!Files.exists(nugetPath)) {
      throw new IllegalStateException("Unable to find NuGet at '" + nugetPath.toString() +
        "'. Please configure property '" + NUGET_PATH + "'");
    }
    return nugetPath;
  }

  @CheckForNull
  public static WsMeasures.Measure getMeasure(Orchestrator orch, String componentKey, String metricKey) {
    WsMeasures.ComponentWsResponse response = newWsClient(orch).measures().component(new ComponentWsRequest()
      .setComponentKey(componentKey)
      .setMetricKeys(singletonList(metricKey)));
    List<Measure> measures = response.getComponent().getMeasuresList();
    return measures.size() == 1 ? measures.get(0) : null;
  }

  @CheckForNull
  public static Integer getMeasureAsInt(Orchestrator orch, String componentKey, String metricKey) {
    Measure measure = getMeasure(orch, componentKey, metricKey);
    return (measure == null) ? null : Integer.parseInt(measure.getValue());
  }

  public static WsComponents.Component getComponent(Orchestrator orch, String componentKey) {
    return newWsClient(orch).components().show(new ShowWsRequest().setKey(componentKey)).getComponent();
  }

  public static List<Issues.Issue> getIssues(Orchestrator orch, String componentKey) {
    return newWsClient(orch).issues().search(new SearchWsRequest().setComponentKeys(Collections.singletonList(componentKey))).getIssuesList();
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
      return "LATEST_RELEASE[6.7]";
    }
    return version;
  }

  public static boolean hasModules(Orchestrator orch) {
    return !orch.getServer().version().isGreaterThanOrEquals(7, 6);
  }

  static WsClient newWsClient(Orchestrator orch) {
    return WsClientFactories.getDefault().newClient(HttpConnector.newBuilder()
      .url(orch.getServer().getUrl())
      .build());
  }
}
