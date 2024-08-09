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
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Collectors;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.sonarqube.ws.Issues;
import org.sonarqube.ws.Measures.Measure;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasure;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;

@ExtendWith(Tests.class)
public class MultipleProjectsTest {

  @TempDir
  private static Path temp;

  private static final String PROJECT = "MultipleProjects";

  private static final String FIRST_PROJECT_DIRECTORY = "MultipleProjects:FirstProject";
  private static final String FIRST_PROJECT_FIRST_CLASS_FILE = "MultipleProjects:FirstProject/FirstClass.cs";
  private static final String FIRST_PROJECT_SECOND_CLASS_FILE = "MultipleProjects:FirstProject/SecondClass.cs";

  private static final String SECOND_PROJECT_DIRECTORY = "MultipleProjects:SecondProject";
  private static final String SECOND_PROJECT_FIRST_CLASS_FILE = "MultipleProjects:SecondProject/FirstClass.cs";

  private static BuildResult buildResult;

  @BeforeAll
  public static void beforeAll() throws Exception {
    Path projectDir = TestUtils.projectDir(temp, PROJECT);
    ScannerForMSBuild beginStep = TestUtils.createBeginStep(PROJECT, projectDir);

    ORCHESTRATOR.executeBuild(beginStep);
    TestUtils.runBuild(projectDir);
    buildResult = ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));
  }

  @Test
  void projectTypesInfoIsLogged() {
    assertThat(buildResult.getLogs()).contains("Found 2 MSBuild C# projects: 2 MAIN projects.");
  }

  @Test
  void roslynVersionIsLogged() {
    // FirstProject + 2x SecondProject (each target framework has its log)
    assertThat(buildResult.getLogsLines(x -> x.startsWith("INFO: Roslyn version: "))).hasSize(3);
  }

  @Test
  void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo("MultipleProjects");

    assertThat(getComponent(FIRST_PROJECT_DIRECTORY).getName()).isEqualTo("FirstProject");
    assertThat(getComponent(FIRST_PROJECT_FIRST_CLASS_FILE).getName()).isEqualTo("FirstClass.cs");
    assertThat(getComponent(FIRST_PROJECT_SECOND_CLASS_FILE).getName()).isEqualTo("SecondClass.cs");

    assertThat(getComponent(SECOND_PROJECT_DIRECTORY).getName()).isEqualTo("SecondProject");
    assertThat(getComponent(SECOND_PROJECT_FIRST_CLASS_FILE).getName()).isEqualTo("FirstClass.cs");
  }

  @Test
  void firstProjectFirstClassIssues() {
    List<Issues.Issue> barIssues = getIssues(FIRST_PROJECT_FIRST_CLASS_FILE)
      .stream()
      .filter(x -> x.getRule().startsWith("csharpsquid:"))
      .collect(Collectors.toList());

    assertThat(barIssues)
      .hasSize(3)
      .extracting(Issues.Issue::getRule, Issues.Issue::getLine)
      .containsExactlyInAnyOrder(
        tuple("csharpsquid:S1134", 3),
        tuple("csharpsquid:S1135", 4),
        tuple("csharpsquid:S2325", 16));
  }

  @Test
  void secondProjectFirstClassIssues() {
    List<Issues.Issue> barIssues = getIssues(SECOND_PROJECT_FIRST_CLASS_FILE)
      .stream()
      .filter(x -> x.getRule().startsWith("csharpsquid:"))
      .collect(Collectors.toList());

    assertThat(barIssues)
      .extracting(Issues.Issue::getRule, Issues.Issue::getLine)
      .containsExactlyInAnyOrder(
        tuple("csharpsquid:S1135", 1),
        tuple("csharpsquid:S2325", 10));
  }

  /* Files */

  @Test
  void filesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("files")).isEqualTo(3);
  }

  @Test
  void filesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "files")).isEqualTo(2);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "files")).isEqualTo(1);
  }

  @Test
  void filesAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "files")).isEqualTo(1);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "files")).isEqualTo(1);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "files")).isEqualTo(1);
  }

  /* Statements */

  @Test
  void statementsAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("statements")).isEqualTo(4);
  }

  @Test
  void statementsAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "statements")).isEqualTo(3);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "statements")).isEqualTo(1);
  }

  @Test
  void statementsAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "statements")).isEqualTo(3);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "statements")).isZero();

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "statements")).isOne();
  }

  /* Complexity */

  @Test
  void complexityAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("complexity")).isEqualTo(8);
  }

  @Test
  void complexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "complexity")).isEqualTo(6);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "complexity")).isEqualTo(2);
  }

  @Test
  void complexityAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "complexity")).isEqualTo(5);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "complexity")).isEqualTo(1);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "complexity")).isEqualTo(2);
  }

  /* Cognitive Complexity */

  @Test
  void cognitiveComplexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "cognitive_complexity")).isEqualTo(4);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "cognitive_complexity")).isZero();
  }

  @Test
  void cognitiveComplexityAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("cognitive_complexity")).isEqualTo(4);
  }

  @Test
  void cognitiveComplexityAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "cognitive_complexity")).isEqualTo(4);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "cognitive_complexity")).isZero();

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "cognitive_complexity")).isZero();
  }

  /* Lines */

  @Test
  void linesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("lines")).isEqualTo(50);
  }

  @Test
  void linesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "lines")).isEqualTo(35);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "lines")).isEqualTo(15);
  }

  @Test
  void linesAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "lines")).isEqualTo(28);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "lines")).isEqualTo(7);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "lines")).isEqualTo(15);
  }

  /* Lines of code */

  @Test
  void linesOfCodeAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("ncloc")).isEqualTo(35);
  }

  @Test
  void linesOfCodeAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "ncloc")).isEqualTo(25);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "ncloc")).isEqualTo(10);
  }

  @Test
  void linesOfCodeAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "ncloc")).isEqualTo(20);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "ncloc")).isEqualTo(5);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "ncloc")).isEqualTo(10);
  }

  /* Comment lines */


  @Test
  void commentLinesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("comment_lines")).isEqualTo(4);
  }

  @Test
  void commentLinesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "comment_lines")).isEqualTo(4);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "comment_lines")).isEqualTo(0);
  }

  @Test
  void commentLinesAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "comment_lines")).isEqualTo(3);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "comment_lines")).isEqualTo(1);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "comment_lines")).isZero(); // The leading one is not counted
  }

  /* Functions */

  @Test
  void functionsAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("functions")).isEqualTo(6);
  }

  @Test
  void functionsAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "functions")).isEqualTo(4);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "functions")).isEqualTo(2);
  }

  @Test
  void functionsAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "functions")).isEqualTo(3);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "functions")).isEqualTo(1);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "functions")).isEqualTo(2);
  }

  /* Classes */

  @Test
  void classesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("classes")).isEqualTo(3);
  }

  @Test
  void classesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt(FIRST_PROJECT_DIRECTORY, "classes")).isEqualTo(2);
    assertThat(getDirectoryMeasureAsInt(SECOND_PROJECT_DIRECTORY, "classes")).isEqualTo(1);
  }

  @Test
  void classesAtFileLevel() {
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_FIRST_CLASS_FILE, "classes")).isEqualTo(1);
    assertThat(getFileMeasureAsInt(FIRST_PROJECT_SECOND_CLASS_FILE, "classes")).isEqualTo(1);

    assertThat(getFileMeasureAsInt(SECOND_PROJECT_FIRST_CLASS_FILE, "classes")).isEqualTo(1);
  }

  @Test
  void linesOfCodeByLine() {
    String firstProjectFirstClass = getFileMeasure(FIRST_PROJECT_FIRST_CLASS_FILE, "ncloc_data").getValue();
    assertThat(firstProjectFirstClass).isEqualTo("1=1;5=1;7=1;8=1;9=1;11=1;12=1;14=1;16=1;17=1;18=1;19=1;20=1;21=1;22=1;23=1;24=1;25=1;26=1;27=1");

    String secondProjectFirstClassNcloc = getFileMeasure(SECOND_PROJECT_FIRST_CLASS_FILE, "ncloc_data").getValue();
    assertThat(secondProjectFirstClassNcloc).isEqualTo("2=1;4=1;6=1;7=1;8=1;10=1;11=1;12=1;13=1;14=1");
  }

  /* Helper methods */

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT, metricKey);
  }

  private Integer getDirectoryMeasureAsInt(String directory, String metricKey) {
    return getMeasureAsInt(directory, metricKey);
  }

  private Measure getFileMeasure(String file, String metricKey) {
    return getMeasure(file, metricKey);
  }

  private Integer getFileMeasureAsInt(String file, String metricKey) {
    return getMeasureAsInt(file, metricKey);
  }
}
