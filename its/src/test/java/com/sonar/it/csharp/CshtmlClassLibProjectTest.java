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

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import java.nio.file.Path;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class CshtmlClassLibProjectTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT = "CshtmlClassLib";
  private static final String CSHTML_PAGE_CLASS_FILE = "CshtmlClassLib:Areas/MyFeature/Pages/Page.cshtml";

  @BeforeAll
  public static void beforeAll() throws Exception {
    Path projectDir = TestUtils.projectDir(temp, PROJECT);
    ScannerForMSBuild beginStep = TestUtils.createBeginStep(PROJECT, projectDir);

    ORCHESTRATOR.executeBuild(beginStep);
    TestUtils.runBuild(projectDir);
    var buildResult = ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));
  }

  @Test
  void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo("CshtmlClassLib");

    assertThat(getComponent(CSHTML_PAGE_CLASS_FILE).getName()).isEqualTo("Page.cshtml");
  }
}
