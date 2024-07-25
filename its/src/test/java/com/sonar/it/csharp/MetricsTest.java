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

import java.nio.file.Path;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Measures.Measure;

import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getMeasure;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class MetricsTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT = "MetricsTest";
  private static final String DIRECTORY = "MetricsTest:foo";
  private static final String FILE = "MetricsTest:foo/Class1.cs";

  @BeforeAll
  public static void beforeAll() throws Exception {
    // Without setting the testProjectPattern, the MetricsTest project is considered as a Test project :)
    Tests.analyzeProject(temp, PROJECT, "sonar.msbuild.testProjectPattern", "noTests");
  }

  @Test
  void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo(PROJECT);
    assertThat(getComponent(DIRECTORY).getName()).isEqualTo("foo");
    assertThat(getComponent(FILE).getName()).isEqualTo("Class1.cs");
  }

  /* Files */

  @Test
  void filesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("files")).isEqualTo(3);
  }

  @Test
  void filesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("files")).isEqualTo(2);
  }

  @Test
  void filesAtFileLevel() {
    assertThat(getFileMeasureAsInt("files")).isEqualTo(1);
  }

  /* Statements */

  @Test
  void statementsAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("statements")).isEqualTo(15);
  }

  @Test
  void statementsAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("statements")).isEqualTo(10);
  }

  @Test
  void statementsAtFileLevel() {
    assertThat(getFileMeasureAsInt("statements")).isEqualTo(5);
  }

  /* Complexity */

  @Test
  void complexityAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("complexity")).isEqualTo(9);
  }

  @Test
  void complexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("complexity")).isEqualTo(6);
  }

  @Test
  void complexityAtFileLevel() {
    assertThat(getFileMeasureAsInt("complexity")).isEqualTo(3);
  }

  /* Cognitive Complexity */

  @Test
  void cognitiveComplexityAtFileLevel() {
    assertThat(getFileMeasureAsInt("cognitive_complexity")).isEqualTo(1);
  }

  @Test
  void cognitiveComplexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("cognitive_complexity")).isEqualTo(2);
  }

  @Test
  void cognitiveComplexityAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("cognitive_complexity")).isEqualTo(3);
  }

  /* Lines */

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

  /* Lines of code */

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

  /* Comment lines */

  @Test
  void commentLinesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("comment_lines")).isEqualTo(12);
  }

  @Test
  void commentLinesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("comment_lines")).isEqualTo(8);
  }

  @Test
  void commentLinesAtFileLevel() {
    assertThat(getFileMeasureAsInt("comment_lines")).isEqualTo(4);
  }

  /* Functions */

  @Test
  void functionsAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("functions")).isEqualTo(6);
  }

  @Test
  void functionsAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("functions")).isEqualTo(4);
  }

  @Test
  void functionsAtFileLevel() {
    assertThat(getFileMeasureAsInt("functions")).isEqualTo(2);
  }

  /* Classes */

  @Test
  void classesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("classes")).isEqualTo(6);
  }

  @Test
  void classesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("classes")).isEqualTo(4);
  }

  @Test
  void classesAtFileLevel() {
    assertThat(getFileMeasureAsInt("classes")).isEqualTo(2);
  }


  @Test
  void linesOfCodeByLine() {
    String value = getFileMeasure("ncloc_data").getValue();

    assertThat(value)
      .contains("5=1")
      .contains("6=1")
      .contains("7=1")
      .contains("8=1")
      .contains("9=1")
      // lines 10-19
      .contains("13=1")
      .contains("14=1")
      .contains("15=1")
      .contains("16=1")
      // lines 20-29
      .contains("20=1")
      .contains("21=1")
      .contains("22=1")
      .contains("23=1")
      .contains("24=1")
      .contains("25=1")
      .contains("26=1")
      .contains("27=1")
      .contains("29=1")
      // lines 30-39
      .contains("30=1")
      .contains("31=1")
      .contains("32=1")
      .contains("33=1")
      .contains("34=1")
      .contains("35=1")
      .contains("36=1")
      .contains("37=1")
      .contains("38=1")
      .hasSize(144); // No other line
  }

  /* Executable lines */

  @Test
  void executableLines() {

    String value = getFileMeasure("executable_lines_data").getValue();

    assertThat(value)
      .contains("24=1")
      .contains("34=1")
      .contains("36=1")
      .contains("37=1")
      .hasSize(19); // No other lines
  }

  /* Helper methods */

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT, metricKey);
  }

  private Integer getDirectoryMeasureAsInt(String metricKey) {
    return getMeasureAsInt(DIRECTORY, metricKey);
  }

  private Measure getFileMeasure(String metricKey) {
    return getMeasure(FILE, metricKey);
  }

  private Integer getFileMeasureAsInt(String metricKey) {
    return getMeasureAsInt(FILE, metricKey);
  }
}
