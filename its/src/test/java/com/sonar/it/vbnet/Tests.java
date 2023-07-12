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
package com.sonar.it.vbnet;

import com.sonar.orchestrator.container.Edition;
import com.sonar.orchestrator.junit5.OrchestratorExtension;
import com.sonar.orchestrator.junit5.OrchestratorExtensionBuilder;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import java.io.File;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.platform.suite.api.SelectPackages;
import org.junit.platform.suite.api.Suite;

@Suite
@SelectPackages("com.sonar.it.vbnet") // This will run all classes from current package containing @Test methods.
public class Tests {

  public static final OrchestratorExtension ORCHESTRATOR = prepareOrchestrator()
    .addPlugin(getPluginLocation("sonar-csharp-plugin")) // Do not add VB.NET here, use shared project instead
    .restoreProfileAtStartup(FileLocation.of("profiles/no_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/class_name.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/template_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_parameters.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_complexity.xml"))
    .build();

  public static OrchestratorExtensionBuilder prepareOrchestrator() {
    System.out.println("prepareOrchestrator");

    return OrchestratorExtension.builderEnv()
      .useDefaultAdminCredentialsForBuilds(true)
      // See https://github.com/SonarSource/orchestrator#version-aliases
      .setSonarVersion(System.getProperty("sonar.runtimeVersion", "DEV"))
      .setEdition(Edition.DEVELOPER)
      .activateLicense();
  }

  public static Location getPluginLocation(String pluginName) {
    Location pluginLocation;

    String version = System.getProperty("csharpVersion"); // C# and VB.Net versions are the same
    return FileLocation.byWildcardMavenFilename(new File("../" + pluginName + "/target"), pluginName + "-*.jar");
  }


  // ToDo: This is a temporary workaround for jUnit5 migration that should be removed in https://github.com/SonarSource/sonar-dotnet/pull/7574
  @BeforeAll
  public static void startOrchestrator() {
    System.out.println("Start Orchestrator VB.NET");
  }

  // ToDo: This is a temporary workaround for jUnit5 migration that should be removed in https://github.com/SonarSource/sonar-dotnet/pull/7574
  @AfterAll
  public static void stopOrchestrator() {
    System.out.println("Stop Orchestrator VB.NET");
  }
}
