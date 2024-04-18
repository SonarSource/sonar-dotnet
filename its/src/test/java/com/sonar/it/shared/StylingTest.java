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

import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import java.util.List;
import org.apache.commons.io.FileUtils;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;

@ExtendWith(Tests.class)
public class StylingTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT = "Styling";
  private static final String STYLING_DLL = "Internal.SonarAnalyzer.CSharp.Styling.dll";

  @Test
  public void hasIssues() throws IOException {
    Path projectFullPath = TestUtils.projectDir(temp, PROJECT);
    FileUtils.copyFile(new File("..\\analyzers\\packaging\\binaries\\internal\\" + STYLING_DLL),
      projectFullPath.resolve(STYLING_DLL).toFile());
    Tests.ORCHESTRATOR.executeBuild(TestUtils.createBeginStep(PROJECT, projectFullPath));
    TestUtils.runBuild(projectFullPath);
    Tests.ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectFullPath));

    List<Issues.Issue> issues = TestUtils.getIssues(Tests.ORCHESTRATOR, PROJECT);
    assertThat(issues)
      .extracting(Issues.Issue::getRule, Issues.Issue::getComponent, Issues.Issue::getLine)
      .containsExactlyInAnyOrder(
        tuple("csharpsquid:S1134", "Styling:File.cs", 1),
        tuple("external_roslyn:T0001", "Styling:File.cs", 3));
  }
}
