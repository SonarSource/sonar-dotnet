/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2019 SonarSource SA
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
import java.io.File;
import java.io.IOException;
import java.nio.file.Path;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.SystemUtils;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.ExternalResource;
import org.junit.rules.RuleChain;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.WsMeasures.Measure;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getMeasure;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.apache.commons.lang.StringUtils.countMatches;
import static org.assertj.core.api.Assertions.assertThat;

public class MetricsTest {

  public static TemporaryFolder temp = new TemporaryFolder();

  private static final String PROJECT = "MetricsTest";
  private static final String DIRECTORY = TestUtils.hasModules(ORCHESTRATOR) ? "MetricsTest:MetricsTest:1F026ECA-900A-488D-9D07-AD23216FA32B:foo" : "MetricsTest:foo";
  private static final String FILE = TestUtils.hasModules(ORCHESTRATOR) ? "MetricsTest:MetricsTest:1F026ECA-900A-488D-9D07-AD23216FA32B:foo/Class1.cs" : "MetricsTest:foo/Class1.cs";

  @ClassRule
  public static RuleChain chain = getRuleChain();

  private static RuleChain getRuleChain() {
    assertThat(SystemUtils.IS_OS_WINDOWS).withFailMessage("OS should be Windows.").isTrue();

    // Scanner for MSBuild caches the analyzer, so running the test twice in a row means the old binary is used.
    // This code deletes the cache, but there should be a way to run without cache.
    // Ticket: https://jira.sonarsource.com/browse/SONARMSBRU-346
    String localAppData = System.getenv("LOCALAPPDATA") + "\\Temp\\.sonarqube";
    try {
      FileUtils.deleteDirectory(new File(localAppData));
    }
    catch (IOException ioe) {
      throw new IllegalStateException("could not delete Scanner for MSBuild cache folder", ioe);
    }

    return RuleChain
      .outerRule(ORCHESTRATOR)
      .around(temp)
      .around(new ExternalResource() {
        @Override
        protected void before() throws Throwable {
          ORCHESTRATOR.resetData();

          Path projectDir = Tests.projectDir(temp, "MetricsTest");
          ORCHESTRATOR.executeBuild(TestUtils.newScanner(projectDir)
            .addArgument("begin")
            .setProjectKey("MetricsTest")
            .setProjectName("MetricsTest")
            .setProjectVersion("1.0")
            .setProfile("no_rule")
            // Without that, the MetricsTest project is considered as a Test project :)
            .setProperty("sonar.msbuild.testProjectPattern", "noTests"));

          TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Rebuild");

          ORCHESTRATOR.executeBuild(TestUtils.newScanner(projectDir)
            .addArgument("end"));
        }
      });
  }

  @Test
  public void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo("MetricsTest");
    assertThat(getComponent(DIRECTORY).getName()).isEqualTo("foo");
    assertThat(getComponent(FILE).getName()).isEqualTo("Class1.cs");
  }

  /* Files */

  @Test
  public void filesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("files")).isEqualTo(3);
  }

  @Test
  public void filesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("files")).isEqualTo(2);
  }

  @Test
  public void filesAtFileLevel() {
    assertThat(getFileMeasureAsInt("files")).isEqualTo(1);
  }

  /* Statements */

  @Test
  public void statementsAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("statements")).isEqualTo(15);
  }

  @Test
  public void statementsAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("statements")).isEqualTo(10);
  }

  @Test
  public void statementsAtFileLevel() {
    assertThat(getFileMeasureAsInt("statements")).isEqualTo(5);
  }

  /* Complexity */

  @Test
  public void complexityAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("complexity")).isEqualTo(9);
  }

  @Test
  public void complexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("complexity")).isEqualTo(6);
  }

  @Test
  public void complexityAtFileLevel() {
    assertThat(getFileMeasureAsInt("complexity")).isEqualTo(3);
  }

  @Test
  public void complexityInClassesAtFileLevel() {
    assertThat(getFileMeasureAsInt("complexity_in_classes")).isEqualTo(3);
  }

  @Test
  public void complexityInFunctionsAtFileLevel() {
    assertThat(getFileMeasureAsInt("complexity_in_functions")).isEqualTo(3);
  }

  /* Cognitive Complexity */

  @Test
  public void cognitiveComplexityAtFileLevel() {
    assertThat(getFileMeasureAsInt("cognitive_complexity")).isEqualTo(1);
  }

  @Test
  public void cognitiveComplexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("cognitive_complexity")).isEqualTo(2);
  }

  @Test
  public void cognitiveComplexityAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("cognitive_complexity")).isEqualTo(3);
  }

  /* Lines */

  @Test
  public void linesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("lines")).isEqualTo(114);
  }

  @Test
  public void linesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("lines")).isEqualTo(76);
  }

  @Test
  public void linesAtFileLevel() {
    assertThat(getFileMeasureAsInt("lines")).isEqualTo(38);
  }

  /* Lines of code */

  @Test
  public void linesOfCodeAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("ncloc")).isEqualTo(90);
  }

  @Test
  public void linesOfCodeAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("ncloc")).isEqualTo(60);
  }

  @Test
  public void linesOfCodeAtFileLevel() {
    assertThat(getFileMeasureAsInt("ncloc")).isEqualTo(30);
  }

  /* Comment lines */

  @Test
  public void commentLinesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("comment_lines")).isEqualTo(12);
  }

  @Test
  public void commentLinesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("comment_lines")).isEqualTo(8);
  }

  @Test
  public void commentLinesAtFileLevel() {
    assertThat(getFileMeasureAsInt("comment_lines")).isEqualTo(4);
  }

  /* Functions */

  @Test
  public void functionsAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("functions")).isEqualTo(6);
  }

  @Test
  public void functionsAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("functions")).isEqualTo(4);
  }

  @Test
  public void functionsAtFileLevel() {
    assertThat(getFileMeasureAsInt("functions")).isEqualTo(2);
  }

  /* Classes */

  @Test
  public void classesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("classes")).isEqualTo(6);
  }

  @Test
  public void classesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("classes")).isEqualTo(4);
  }

  @Test
  public void classesAtFileLevel() {
    assertThat(getFileMeasureAsInt("classes")).isEqualTo(2);
  }

  /* Public API */

  @Test
  public void publicApiAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("public_api")).isEqualTo(6);
    assertThat(getProjectMeasureAsInt("public_undocumented_api")).isEqualTo(3);
  }

  @Test
  public void publicApiAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("public_api")).isEqualTo(4);
    assertThat(getDirectoryMeasureAsInt("public_undocumented_api")).isEqualTo(2);
  }

  @Test
  public void publicApiAtFileLevel() {
    assertThat(getFileMeasureAsInt("public_api")).isEqualTo(2);
    assertThat(getFileMeasureAsInt("public_undocumented_api")).isEqualTo(1);
  }

  /* Complexity distribution */

  @Test
  public void complexityDistributionAtProjectLevel() {
    assertThat(getProjectMeasure("function_complexity_distribution").getValue()).isEqualTo("1=3;2=3;4=0;6=0;8=0;10=0;12=0");
    assertThat(getDirectoryMeasure("file_complexity_distribution").getValue()).isEqualTo("0=2;5=0;10=0;20=0;30=0;60=0;90=0");
  }

  @Test
  public void complexityDistributionAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("function_complexity_distribution").getValue()).isEqualTo("1=2;2=2;4=0;6=0;8=0;10=0;12=0");
    assertThat(getDirectoryMeasure("file_complexity_distribution").getValue()).isEqualTo("0=2;5=0;10=0;20=0;30=0;60=0;90=0");
  }

  @Test
  public void complexityDistributionAtFileLevel() {
    assertThat(getFileMeasureAsInt("function_complexity_distribution")).isNull();
    assertThat(getFileMeasureAsInt("file_complexity_distribution")).isNull();
  }

  @Test
  public void linesOfCodeByLine() {
    String value = getFileMeasure("ncloc_data").getValue();

    assertThat(value).contains("1=1");
    assertThat(value).contains("2=1");
    assertThat(value).contains("3=1");
    assertThat(value).contains("4=1");
    assertThat(value).contains("5=1");

    assertThat(value).contains("9=1");
    assertThat(value).contains("10=1");
    assertThat(value).contains("11=1");
    assertThat(value).contains("12=1");

    assertThat(value).contains("16=1");
    assertThat(value).contains("17=1");
    assertThat(value).contains("18=1");
    assertThat(value).contains("19=1");
    assertThat(value).contains("20=1");
    assertThat(value).contains("21=1");
    assertThat(value).contains("22=1");
    assertThat(value).contains("23=1");

    assertThat(value).contains("25=1");
    assertThat(value).contains("26=1");
    assertThat(value).contains("27=1");
    assertThat(value).contains("28=1");
    assertThat(value).contains("29=1");
    assertThat(value).contains("30=1");
    assertThat(value).contains("31=1");
    assertThat(value).contains("32=1");
    assertThat(value).contains("33=1");
    assertThat(value).contains("34=1");

    assertThat(value.length()).isEqualTo(143); // No other line
  }

  /* Executable lines */

  @Test
  public void executableLines() {

    String value = getFileMeasure("executable_lines_data").getValue();

    assertThat(value).contains("20=1");
    assertThat(value).contains("30=1");
    assertThat(value).contains("32=1");
    assertThat(value).contains("33=1");

    assertThat(value.length()).isEqualTo(19); // No other lines
  }

  /* Helper methods */

  private Measure getProjectMeasure(String metricKey) {
    return getMeasure(PROJECT, metricKey);
  }

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT, metricKey);
  }

  private Measure getDirectoryMeasure(String metricKey) {
    return getMeasure(DIRECTORY, metricKey);
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
