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
package com.sonar.it.vbnet;

import com.sonar.it.shared.TestUtils;
import java.nio.file.Path;
import java.util.List;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Duplications;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class CodeDuplicationTest {
  @TempDir
  private static Path temp;
  private static final String PROJECT = "VbCodeDuplicationTest";

  @BeforeAll
  public static void init() throws Exception {
    Tests.analyzeProject(temp, PROJECT);
  }

  @Test
  public void codeDuplicationResultsAreImportedForMainCode() throws Exception {
    List<Duplications.Duplication> duplications = TestUtils.getDuplication(ORCHESTRATOR, "VbCodeDuplicationTest:VbCodeDuplicationMainProj/DuplicatedMainClass1.vb").getDuplicationsList();
    assertThat(duplications).hasSize(1);
    Duplications.Duplication duplication = duplications.get(0);
    assertThat(duplication.getBlocksCount()).isEqualTo(2);
    Duplications.Block firstBlock = duplication.getBlocks(0);
    Duplications.Block secondBlock = duplication.getBlocks(1);
    assertThat(firstBlock.getFrom()).isEqualTo(3);
    assertThat(firstBlock.getSize()).isEqualTo(55);
    assertThat(secondBlock.getFrom()).isEqualTo(3);
    assertThat(secondBlock.getSize()).isEqualTo(55);
  }

  @Test
  public void codeDuplicationResultsAreNotImportedForTestCode() throws Exception {
    assertThat(TestUtils.getDuplication(ORCHESTRATOR, "VbCodeDuplicationTest:VbCodeDuplicationTestProj/DuplicatedTestClass1.vb").getDuplicationsList()).isEmpty();
  }
}
