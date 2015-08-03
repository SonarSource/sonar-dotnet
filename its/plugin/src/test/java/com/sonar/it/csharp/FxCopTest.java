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
import org.sonar.wsclient.issue.Issue;
import org.sonar.wsclient.issue.IssueQuery;

import java.io.File;
import java.util.List;

import static org.fest.assertions.Assertions.assertThat;

public class FxCopTest {

  @ClassRule
  public static Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @BeforeClass
  public static void init() throws Exception {
    orchestrator.resetData();
  }

  @Test
  public void should_import_fxcop_reports() {
    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/FxCopTest"))
      .setProjectKey("ImportReport")
      .setProjectName("ImportReport")
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProperty("sonar.modules", "CSharp")
      .setProperty("sonar.cs.fxcop.reportPath", "import-report.xml")
      .setProperty("sonar.sourceEncoding", "UTF-8")
      .setProfile("fxcop");

    orchestrator.executeBuild(build);

    List<Issue> issues = orchestrator.getServer().wsClient().issueClient().find(IssueQuery.create().componentRoots("ImportReport")).list();

    assertThat(issues).hasSize(1);
  }

}
