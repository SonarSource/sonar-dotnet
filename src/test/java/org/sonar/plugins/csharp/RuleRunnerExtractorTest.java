/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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
package org.sonar.plugins.csharp;

import org.junit.Test;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class RuleRunnerExtractorTest {

  @Test
  public void test() {
    ProjectReactor reactor = mock(ProjectReactor.class);
    ProjectDefinition root = mock(ProjectDefinition.class);
    File workDir = new File("target/RuleRunnerExtractorTest/");
    when(root.getWorkDir()).thenReturn(workDir);
    when(reactor.getRoot()).thenReturn(root);

    RuleRunnerExtractor extractor = new RuleRunnerExtractor(reactor);

    assertThat(extractor.executableFile()).isEqualTo(new File(new File(workDir, "SonarLint.Runner"), "SonarLint.Runner.exe"));
    assertThat(extractor.executableFile()).isEqualTo(new File(new File(workDir, "SonarLint.Runner"), "SonarLint.Runner.exe"));
  }

}
