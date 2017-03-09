/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2016-2017 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class SonarAnalyzerScannerExtractorTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Test
  public void test() throws Exception {
    ProjectReactor reactor = mock(ProjectReactor.class);
    ProjectDefinition root = mock(ProjectDefinition.class);
    File workDir = temp.newFolder();
    when(root.getWorkDir()).thenReturn(workDir);
    when(reactor.getRoot()).thenReturn(root);

    SonarAnalyzerScannerExtractor extractor = new SonarAnalyzerScannerExtractor(reactor);

    File expectedExe = new File(new File(workDir, "SonarAnalyzer.Scanner/vbnet"), "SonarAnalyzer.Scanner.exe");
    assertThat(extractor.executableFile("vbnet")).isEqualTo(expectedExe);
    assertThat(extractor.executableFile("vbnet")).isEqualTo(expectedExe);
  }

}
