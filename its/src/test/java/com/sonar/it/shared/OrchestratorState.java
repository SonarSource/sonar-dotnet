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
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import org.apache.commons.io.FileUtils;

public class OrchestratorState {

  private final Orchestrator orchestrator;
  private static volatile boolean cacheDeleted;
  private volatile int usageCount;

  public OrchestratorState(Orchestrator orchestrator) {
    this.orchestrator = orchestrator;
  }

  public void startOnce() throws IOException {
    synchronized (OrchestratorState.class) {
      if (!cacheDeleted) {
        TestUtils.deleteLocalCache();
        cacheDeleted = true;
      }
      usageCount += 1;
      if (usageCount == 1) {
        orchestrator.start();
        // To avoid a race condition in scanner file cache mechanism we analyze single project before any test to populate the cache
        analyzeEmptyProject();
      }
    }
  }

  public void stopOnce() throws Exception {
    synchronized (OrchestratorState.class) {
      usageCount -= 1;
      if (usageCount == 0) {
        orchestrator.stop();
      }
    }
  }

  private void analyzeEmptyProject() throws IOException {
    Path temp = Files.createTempDirectory("OrchestratorStartup." + Thread.currentThread().getName());
    Path projectFullPath = TestUtils.projectDir(temp, "Empty");
    orchestrator.executeBuild(TestUtils.createBeginStep("OrchestratorStateStartup", projectFullPath));
    TestUtils.runBuild(projectFullPath);
    orchestrator.executeBuild(TestUtils.createEndStep(projectFullPath));
    FileUtils.deleteDirectory(temp.toFile());
  }
}
