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
import org.junit.jupiter.api.extension.AfterAllCallback;
import org.junit.jupiter.api.extension.BeforeAllCallback;
import org.junit.jupiter.api.extension.ExtensionContext;

public class OrchestratorStarter implements BeforeAllCallback, AfterAllCallback {

  public static final OrchestratorExtension ORCHESTRATOR = prepareOrchestrator()
    .addPlugin(getPluginLocation("sonar-csharp-plugin")) // Do not add VB.NET here, use shared project instead
    .restoreProfileAtStartup(FileLocation.of("profiles/no_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/class_name.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/template_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_parameters.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_complexity.xml"))
    .build();

  private static volatile int usageCount;

  @Override
  public void beforeAll(ExtensionContext extensionContext) throws Exception {
    System.out.println("beforeAll delegated " + Thread.currentThread().getName());
//
    synchronized (OrchestratorStarter.class) {
      //ORCHESTRATOR.beforeAll(extensionContext.getParent().orElse(extensionContext));
      if (usageCount == 0) {
        // this will register "this.close()" method to be called when GLOBAL context is shutdown
        //extensionContext.getRoot().getStore(GLOBAL).put(OrchestratorStarter.class, this);
        System.out.println("ORCHESTRATOR.start() " + Thread.currentThread().getName());
        ORCHESTRATOR.start();

        // to avoid a race condition in scanner file cache mechanism we analyze single project before any test to populate the cache
        // testProject();
      }
      usageCount += 1;
    }
  }

  @Override
  public void afterAll(ExtensionContext extensionContext) throws Exception {
    System.out.println("afterAll delegated " + Thread.currentThread().getName());

    synchronized (OrchestratorStarter.class) {
      //ORCHESTRATOR.afterAll(extensionContext.getParent().orElse(extensionContext));
      System.out.println("The usageCount was " + usageCount);
      usageCount -= 1;
      if (usageCount == 0) {
        System.out.println("ORCHESTRATOR.end()");
        ORCHESTRATOR.stop();
      }
    }
  }

  private static OrchestratorExtensionBuilder prepareOrchestrator() {
    System.out.println("prepareOrchestrator");

    return OrchestratorExtension.builderEnv()
      .useDefaultAdminCredentialsForBuilds(true)
      // See https://github.com/SonarSource/orchestrator#version-aliases
      .setSonarVersion(System.getProperty("sonar.runtimeVersion", "DEV"))
      .setEdition(Edition.DEVELOPER)
      .activateLicense();
  }

  public static Location getPluginLocation(String pluginName) {
    return FileLocation.byWildcardMavenFilename(new File("../" + pluginName + "/target"), pluginName + "-*.jar");
  }
}
