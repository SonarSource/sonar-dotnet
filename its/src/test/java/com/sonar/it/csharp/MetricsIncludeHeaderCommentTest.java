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
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Path;

import com.sonar.orchestrator.build.ScannerForMSBuild;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.SystemUtils;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.ExternalResource;
import org.junit.rules.RuleChain;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * This is copy pasted from MetricsTest. It does not re-test everything, it just makes sure
 * that when 'ignoreHeaderComments' is set to False, it counts the header comments as well
 * and it does not modify the LOC metrics.
 */
public class MetricsIncludeHeaderCommentTest {

  public static TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String PROJECT = "MetricsTest";
  private static final String DIRECTORY = TestUtils.hasModules(ORCHESTRATOR) ? "MetricsTest:MetricsTest:1F026ECA-900A-488D-9D07-AD23216FA32B:foo" : "MetricsTest:foo";
  private static final String FILE = TestUtils.hasModules(ORCHESTRATOR) ? "MetricsTest:MetricsTest:1F026ECA-900A-488D-9D07-AD23216FA32B:foo/Class1.cs" : "MetricsTest:foo/Class1.cs";
  private static final int NUMBER_OF_HEADER_COMMENT_LINES = 2;

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

          ScannerForMSBuild beginStep = TestUtils.createBeginStep("MetricsTest", projectDir)
            .setProfile("no_rule")
            // Without that, the MetricsTest project is considered as a Test project :)
            .setProperty("sonar.msbuild.testProjectPattern", "noTests");

          ORCHESTRATOR.executeBuild(beginStep);

          setIgnoreHeaderCommentsToFalse(projectDir);

          TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Rebuild");

          ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));
        }
      });
  }

  @Test
  public void projectIsAnalyzed() {
    assertThat(getComponent(PROJECT).getName()).isEqualTo("MetricsTest");
    assertThat(getComponent(DIRECTORY).getName()).isEqualTo("foo");
    assertThat(getComponent(FILE).getName()).isEqualTo("Class1.cs");
  }

  /* Lines - must be the same */

  @Test
  public void linesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("lines")).isEqualTo(118);
  }

  @Test
  public void linesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("lines")).isEqualTo(80);
  }

  @Test
  public void linesAtFileLevel() {
    assertThat(getFileMeasureAsInt("lines")).isEqualTo(42);
  }

  /* Lines of code - must be the same */

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

  /* Comment lines - these are actually modified */

  @Test
  public void commentLinesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("comment_lines")).isEqualTo(12 + NUMBER_OF_HEADER_COMMENT_LINES);
  }

  @Test
  public void commentLinesAtDirectoryLevel() {
    assertThat(getDirectoryMeasureAsInt("comment_lines")).isEqualTo(8 + NUMBER_OF_HEADER_COMMENT_LINES);
  }

  @Test
  public void commentLinesAtFileLevel() {
    assertThat(getFileMeasureAsInt("comment_lines")).isEqualTo(4 + NUMBER_OF_HEADER_COMMENT_LINES);
  }

  /* Helper methods */

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT, metricKey);
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
    try (BufferedReader reader = new BufferedReader(new FileReader(sonarLintXml)))
    {
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
