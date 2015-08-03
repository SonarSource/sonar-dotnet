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
import org.sonar.wsclient.services.ResourceQuery;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;

public class FileSuffixesTest {

  @ClassRule
  public static Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @BeforeClass
  public static void init() throws Exception {
    orchestrator.resetData();
  }

  @Test
  public void suffixes_set_to_cs() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/FileSuffixesTest/"))
      .setProjectKey("FileSuffixesTest")
      .setProjectName("FileSuffixesTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("FileSuffixesTest", "files")).getMeasureIntValue("files")).isEqualTo(1);
  }

  @Test
  public void suffixes_set_to_cs_and_txt() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/FileSuffixesTest/"))
      .setProjectKey("FileSuffixesTest")
      .setProjectName("FileSuffixesTest")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProperty("sonar.cs.file.suffixes", ".cs,.txt")
      .setProfile("no_rule");
    orchestrator.executeBuild(build);

    Sonar wsClient = orchestrator.getServer().getWsClient();
    assertThat(wsClient.find(ResourceQuery.createForMetrics("FileSuffixesTest", "files")).getMeasureIntValue("files")).isEqualTo(2);
  }

}
