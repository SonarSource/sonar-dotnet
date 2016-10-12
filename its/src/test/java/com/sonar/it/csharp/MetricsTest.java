/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011 SonarSource
 * sonarqube@googlegroups.com
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.it.csharp;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.SonarRunner;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.sonar.wsclient.Sonar;
import org.sonar.wsclient.services.Measure;
import org.sonar.wsclient.services.Resource;
import org.sonar.wsclient.services.ResourceQuery;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;

public class MetricsTest {

  private static final String PROJECT = "MetricsTest";
  private static final String DIRECTORY = "MetricsTest:foo";
  private static final String FILE = "MetricsTest:foo/Class1.cs";

  @ClassRule
  public static Orchestrator orchestrator = Tests.ORCHESTRATOR;

  private static Sonar wsClient;

  @BeforeClass
  public static void init() {
    orchestrator.resetData();

    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/MetricsTest/"))
      .setProjectKey("MetricsTest")
      .setProjectName("MetricsTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    wsClient = orchestrator.getServer().getWsClient();
  }

  @Test
  public void projectIsAnalyzed() {
    assertThat(wsClient.find(new ResourceQuery(PROJECT)).getName()).isEqualTo("MetricsTest");
    assertThat(wsClient.find(new ResourceQuery(PROJECT)).getVersion()).isEqualTo("1.0");
    assertThat(wsClient.find(new ResourceQuery(DIRECTORY)).getName()).isEqualTo("foo");
    assertThat(wsClient.find(new ResourceQuery(FILE)).getName()).isEqualTo("Class1.cs");
  }

  /* Files */

  @Test
  public void filesAtProjectLevel() {
    assertThat(getProjectMeasure("files").getIntValue()).isEqualTo(3);
  }

  @Test
  public void filesAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("files").getIntValue()).isEqualTo(2);
  }

  @Test
  public void filesAtFileLevel() {
    assertThat(getFileMeasure("files").getIntValue()).isEqualTo(1);
  }

  /* Statements */

  @Test
  public void statementsAtProjectLevel() {
    assertThat(getProjectMeasure("statements").getIntValue()).isEqualTo(12);
  }

  @Test
  public void statementsAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("statements").getIntValue()).isEqualTo(8);
  }

  @Test
  public void statementsAtFileLevel() {
    assertThat(getFileMeasure("statements").getIntValue()).isEqualTo(4);
  }

  /* Complexity */

  @Test
  public void complexityAtProjectLevel() {
    assertThat(getProjectMeasure("complexity").getIntValue()).isEqualTo(6);
  }

  @Test
  public void complexityAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("complexity").getIntValue()).isEqualTo(4);
  }

  @Test
  public void complexityAtFileLevel() {
    assertThat(getFileMeasure("complexity").getIntValue()).isEqualTo(2);
  }

  @Test
  public void complexityInClassesAtFileLevel() {
    assertThat(getFileMeasure("complexity_in_classes").getIntValue()).isEqualTo(2);
  }

  @Test
  public void complexityInFunctionsAtFileLevel() {
    assertThat(getFileMeasure("complexity_in_functions").getIntValue()).isEqualTo(2);
  }

  /* Lines */

  @Test
  public void linesAtProjectLevel() {
    assertThat(getProjectMeasure("lines").getIntValue()).isEqualTo(99);
  }

  @Test
  public void linesAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("lines").getIntValue()).isEqualTo(66);
  }

  @Test
  public void linesAtFileLevel() {
    assertThat(getFileMeasure("lines").getIntValue()).isEqualTo(33);
  }

  /* Lines of code */

  @Test
  public void linesOfCodeAtProjectLevel() {
    assertThat(getProjectMeasure("ncloc").getIntValue()).isEqualTo(81);
  }

  @Test
  public void linesOfCodeAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("ncloc").getIntValue()).isEqualTo(54);
  }

  @Test
  public void linesOfCodeAtFileLevel() {
    assertThat(getFileMeasure("ncloc").getIntValue()).isEqualTo(27);
  }

  /* Comment lines */

  @Test
  public void commentLinesAtProjectLevel() {
    assertThat(getProjectMeasure("comment_lines").getIntValue()).isEqualTo(6);
  }

  @Test
  public void commentLinesAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("comment_lines").getIntValue()).isEqualTo(4);
  }

  @Test
  public void commentLinesAtFileLevel() {
    assertThat(getFileMeasure("comment_lines").getIntValue()).isEqualTo(2);
  }

  /* Functions */

  @Test
  public void functionsAtProjectLevel() {
    assertThat(getProjectMeasure("functions").getIntValue()).isEqualTo(6);
  }

  @Test
  public void functionsAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("functions").getIntValue()).isEqualTo(4);
  }

  @Test
  public void functionsAtFileLevel() {
    assertThat(getFileMeasure("functions").getIntValue()).isEqualTo(2);
  }

  /* Classes */

  @Test
  public void classesAtProjectLevel() {
    assertThat(getProjectMeasure("classes").getIntValue()).isEqualTo(6);
  }

  @Test
  public void classesAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("classes").getIntValue()).isEqualTo(4);
  }

  @Test
  public void classesAtFileLevel() {
    assertThat(getFileMeasure("classes").getIntValue()).isEqualTo(2);
  }

  /* Public API */

  @Test
  public void publicApiAtProjectLevel() {
    assertThat(getProjectMeasure("public_api").getIntValue()).isEqualTo(6);
    assertThat(getProjectMeasure("public_undocumented_api").getIntValue()).isEqualTo(3);
  }

  @Test
  public void publicApiAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("public_api").getIntValue()).isEqualTo(4);
    assertThat(getDirectoryMeasure("public_undocumented_api").getIntValue()).isEqualTo(2);
  }

  @Test
  public void publicApiAtFileLevel() {
    assertThat(getFileMeasure("public_api").getIntValue()).isEqualTo(2);
    assertThat(getFileMeasure("public_undocumented_api").getIntValue()).isEqualTo(1);
  }

  /* Complexity distribution */

  @Test
  public void complexityDistributionAtProjectLevel() {
    assertThat(getProjectMeasure("function_complexity_distribution").getData()).isEqualTo("1=6;2=0;4=0;6=0;8=0;10=0;12=0");
    assertThat(getDirectoryMeasure("file_complexity_distribution").getData()).isEqualTo("0=2;5=0;10=0;20=0;30=0;60=0;90=0");
  }

  @Test
  public void complexityDistributionAtDirectoryLevel() {
    assertThat(getDirectoryMeasure("function_complexity_distribution").getData()).isEqualTo("1=4;2=0;4=0;6=0;8=0;10=0;12=0");
    assertThat(getDirectoryMeasure("file_complexity_distribution").getData()).isEqualTo("0=2;5=0;10=0;20=0;30=0;60=0;90=0");
  }

  @Test
  public void complexityDistributionAtFileLevel() {
    assertThat(getFileMeasure("function_complexity_distribution")).isNull();
    assertThat(getFileMeasure("file_complexity_distribution")).isNull();
  }

  @Test
  public void linesOfCodeByLine() {
    String value = getFileMeasure("ncloc_data").getData();

    assertThat(value).contains("1=1");
    assertThat(value).contains("2=1");
    assertThat(value).contains("3=1");
    assertThat(value).contains("4=1");
    assertThat(value).contains("5=1");

    assertThat(value).contains("9=1");
    assertThat(value).contains("10=1");

    assertThat(value).contains("12=1");
    assertThat(value).contains("13=1");
    assertThat(value).contains("14=1");
    assertThat(value).contains("15=1");
    assertThat(value).contains("16=1");
    assertThat(value).contains("17=1");
    assertThat(value).contains("18=1");
    assertThat(value).contains("19=1");
    assertThat(value).contains("20=1");
    assertThat(value).contains("21=1");

    assertThat(value).contains("23=1");
    assertThat(value).contains("24=1");
    assertThat(value).contains("25=1");
    assertThat(value).contains("26=1");
    assertThat(value).contains("27=1");
    assertThat(value).contains("28=1");
    assertThat(value).contains("29=1");
    assertThat(value).contains("30=1");
    assertThat(value).contains("31=1");
    assertThat(value).contains("32=1");

    assertThat(value.length()).isEqualTo(128); // No other line
  }

  @Test
  public void commentsByLine() {
    assertThat(getFileMeasure("comment_lines_data").getData()).contains("7=1");
    assertThat(getFileMeasure("comment_lines_data").getData()).contains("11=1");
    assertThat(org.apache.commons.lang.StringUtils.countMatches(getFileMeasure("comment_lines_data").getData(), "=1")).isEqualTo(2);
  }

  /* Helper methods */

  private Measure getProjectMeasure(String metricKey) {
    Resource resource = wsClient.find(ResourceQuery.createForMetrics(PROJECT, metricKey));
    return resource == null ? null : resource.getMeasure(metricKey);
  }

  private Measure getDirectoryMeasure(String metricKey) {
    Resource resource = wsClient.find(ResourceQuery.createForMetrics(DIRECTORY, metricKey));
    return resource == null ? null : resource.getMeasure(metricKey);
  }

  private Measure getFileMeasure(String metricKey) {
    Resource resource = wsClient.find(ResourceQuery.createForMetrics(FILE, metricKey));
    return resource == null ? null : resource.getMeasure(metricKey);
  }

}
