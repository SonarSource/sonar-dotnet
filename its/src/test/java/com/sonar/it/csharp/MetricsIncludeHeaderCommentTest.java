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
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * This is copy pasted from MetricsTest. It does not re-test everything, it just makes sure
 * that when 'ignoreHeaderComments' is set to False, it counts the header comments as well
 * and it does not modify the LOC metrics.
 */
@ExtendWith(Tests.class)
public class MetricsIncludeHeaderCommentTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT_KEY = "MetricsTestIncludeHeaderComment";
  private static final String DIRECTORY = "MetricsTestIncludeHeaderComment:foo";
  private static final String FILE = "MetricsTestIncludeHeaderComment:foo/Class1.cs";

  private static final int NUMBER_OF_HEADER_COMMENT_LINES = 2;

  @BeforeAll
  public static void beforeAll() throws Exception {
    Path projectDir = TestUtils.projectDir(temp, "MetricsTest");
    ScannerForMSBuild beginStep = TestUtils.createBeginStep(PROJECT_KEY, projectDir)
      // Without that, the MetricsTest project is considered as a Test project :)
      .setProperty("sonar.msbuild.testProjectPattern", "noTests");

    ORCHESTRATOR.executeBuild(beginStep);
    setIgnoreHeaderCommentsToFalse(projectDir);
    TestUtils.runBuild(projectDir);
    ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));
  }

  @Test
  void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT_KEY).getName()).isEqualTo("MetricsTestIncludeHeaderComment");
    assertThat(getComponent(DIRECTORY).getName()).isEqualTo("foo");
    assertThat(getComponent(FILE).getName()).isEqualTo("Class1.cs");
  }

  /* Lines - must be the same */

  @Test
  void linesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("lines")).isEqualTo(118);
  }

  @Test
  void linesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("lines")).isEqualTo(80);
  }

  @Test
  void linesAtFileLevel() {
    assertThat(getFileMeasureAsInt("lines")).isEqualTo(42);
  }

  /* Lines of code - must be the same */

  @Test
  void linesOfCodeAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("ncloc")).isEqualTo(90);
  }

  @Test
  void linesOfCodeAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("ncloc")).isEqualTo(60);
  }

  @Test
  void linesOfCodeAtFileLevel() {
    assertThat(getFileMeasureAsInt("ncloc")).isEqualTo(30);
  }

  /* Comment lines - these are actually modified */

  @Test
  void commentLinesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("comment_lines")).isEqualTo(12 + NUMBER_OF_HEADER_COMMENT_LINES);
  }

  @Test
  void commentLinesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("comment_lines")).isEqualTo(8 + NUMBER_OF_HEADER_COMMENT_LINES);
  }

  @Test
  void commentLinesAtFileLevel() {
    assertThat(getFileMeasureAsInt("comment_lines")).isEqualTo(4 + NUMBER_OF_HEADER_COMMENT_LINES);
  }

  /* Helper methods */

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT_KEY, metricKey);
  }

  private Integer getDirectoryMeasureAsInt(String metricKey) {
    return getMeasureAsInt(DIRECTORY, metricKey);
  }

  private Integer getFileMeasureAsInt(String metricKey) {
    return getMeasureAsInt(FILE, metricKey);
  }

  private static void setIgnoreHeaderCommentsToFalse(Path projectDir) throws IOException {
    StringBuilder sb = new StringBuilder();
    String sonarLintXml = projectDir + "\\.sonarqube\\conf\\cs\\SonarLint.xml";
    try (BufferedReader reader = new BufferedReader(new FileReader(sonarLintXml))) {
      String currentLine;
      while ((currentLine = reader.readLine()) != null) {
        if (currentLine.contains("sonar.cs.ignoreHeaderComments")) {
          sb.append(currentLine);
          currentLine = reader.readLine();
          currentLine = currentLine.replaceAll("true", "false");
          sb.append(currentLine);
        } else {
          sb.append(currentLine);
        }
      }
    }

    String contents = sb.toString();
    try (BufferedWriter writer = new BufferedWriter(new FileWriter(sonarLintXml))) {
      if (contents.length() > 0) {
        writer.write(contents);
      }
    }
  }
}
