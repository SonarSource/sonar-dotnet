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
import kotlin.io.FileSystemException;
import org.junit.jupiter.api.Assumptions;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import java.nio.file.Files;
import java.nio.file.Path;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class SymbolicLinksTest {

  @TempDir
  private static Path temp;

  @Test
  void should_supportS_symbolic_links() throws Exception {
    var sampleComponent = "SymbolicLinks:SymbolicLinks/Sample.cs";
    var linkedComponent = "SymbolicLinks:SymbolicLinks/Links/LinkedSample.cs";
    var projectFullPath = TestUtils.projectDir(temp, "SymbolicLinks");
    var link = projectFullPath.resolve("SymbolicLinks/Links").toAbsolutePath();
    var target = projectFullPath.resolve("Links").toAbsolutePath();;

    try {
      Files.createSymbolicLink(link, target);

      var buildResult = Tests.analyzeProjectPath("SymbolicLinks", projectFullPath, null, "sonar.verbose", "true");

      assertThat(buildResult.isSuccess()).isTrue();

      assertThat(getComponent(sampleComponent)).isNotNull();
      assertThat(getIssues(sampleComponent)).isNotEmpty();
      assertThat(getComponent(linkedComponent)).isNotNull();

      // reproducer: the "LinkedSample.cs" file is not part of the scanner context. Due to this, the issues for symbolic linked files are not imported.
      assertThat(getIssues(linkedComponent)).isEmpty();
      assertThat(buildResult.getLogs()).contains("Skipping issue S1135, input file not found or excluded: " + target.resolve("LinkedSample.cs").toAbsolutePath());
    }
    catch (FileSystemException exception) {
      Assumptions.assumeTrue(false, "Symbolic links can be created only when running with administrator privileges.");
    }
  }
}
