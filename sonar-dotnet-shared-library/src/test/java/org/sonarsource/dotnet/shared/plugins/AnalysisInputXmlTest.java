/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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

import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.rule.RuleKey;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

import static org.assertj.core.api.Assertions.assertThat;

public class AnalysisInputXmlTest {
  private static final String LANGUAGE_KEY = "cs";
  private static final String REPOSITORY_KEY = "csharpsquid";
  private SensorContextTester tester;
  private Path workDir;
  private DefaultInputFile inputFile;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Before
  public void setUp() throws FileNotFoundException {
    workDir = temp.getRoot().toPath();
    File csFile = new File("src/test/resources/Program.cs").getAbsoluteFile();
    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir.toFile());

    inputFile = new TestInputFileBuilder(tester.module().key(), "Program.cs")
      .setLanguage(LANGUAGE_KEY)
      .setMetadata(new FileMetadata().readMetadata(new FileReader(csFile)))
      .build();
    tester.fileSystem().add(inputFile);
  }

  @Test
  public void escapesAnalysisInput() throws Exception {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "S1186"))
      .setParam("param1", "value1")
      .activate()
      .create(RuleKey.of(REPOSITORY_KEY, "S9999"))
      .setParam("param9", "value9")
      .activate()
      .build());

    String sonarlint = AnalysisInputXml.generate(true, false, true, tester, REPOSITORY_KEY, LANGUAGE_KEY, "utf-8");

    assertThat(
      sonarlint
        .replaceAll("\r?\n|\r", "")
        .replaceAll("<File>.*?Program.cs</File>", "<File>Program.cs</File>"))
          .isEqualTo(readFile("src/test/resources/AnalysisInputXmlTest/SonarLint-expected.xml").replaceAll("\r?\n|\r", ""));
  }

  private static String readFile(String fileName) throws Exception {
    return new String(Files.readAllBytes(Paths.get(fileName)));
  }
}
