/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2020 SonarSource SA
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
import java.util.List;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

public class NoSourcesTest {

  @ClassRule
  public static final TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String PROJECT = "ProjectWithNoSources";

  @BeforeClass
  public static void init() throws Exception {
    TestUtils.reset(ORCHESTRATOR);
    Tests.analyzeProject(temp, PROJECT, null);
  }

  @Test
  public void filesAtProjectLevel() {
    List<Issues.Issue> issues = TestUtils.getIssues(ORCHESTRATOR, PROJECT);
    assertThat(issues).hasSize(1).hasOnlyOneElementSatisfying(e ->
    {
      assertThat(e.getLine()).isEqualTo(5);
      assertThat(e.getRule()).isEqualTo("csharpsquid:S1186");
    });
  }

}
