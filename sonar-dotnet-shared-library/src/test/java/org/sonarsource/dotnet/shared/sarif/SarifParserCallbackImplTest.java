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
package org.sonarsource.dotnet.shared.sarif;

import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonarsource.dotnet.shared.plugins.SarifParserCallbackImpl;

import static org.assertj.core.api.Assertions.assertThat;

public class SarifParserCallbackImplTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private SensorContextTester ctx;
  private Map<String, String> repositoryKeyByRoslynRuleKey = new HashMap<>();

  private SarifParserCallbackImpl callback;

  @Before
  public void setUp() {
    ctx = SensorContextTester.create(temp.getRoot().toPath());
    repositoryKeyByRoslynRuleKey.put("rule1", "rule1");
    repositoryKeyByRoslynRuleKey.put("rule2", "rule2");

    // file needs to have a few lines so that the issue is within it's range
    ctx.fileSystem().add(TestInputFileBuilder.create("module1", "file1")
      .setContents("My file\ncontents\nwith some\n lines")
      .build());

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey);
  }

  @Test
  public void should_add_issues() {
    callback.onIssue("rule1", createLocation("file1", 2, 3), Collections.emptyList());
    callback.onIssue("rule2", createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1", "rule2");
  }

  @Test
  public void should_ignore_repeated_issues() {
    callback.onIssue("rule1", createLocation("file1", 2, 3), Collections.emptyList());
    callback.onIssue("rule1", createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1");
  }

  private Location createLocation(String filePath, int line, int column) {
    return createLocation(filePath, line, column, line, column + 1);
  }

  private Location createLocation(String filePath, int startLine, int startColumn, int endLine, int endColumn) {
    return new Location(temp.getRoot().toPath().resolve(filePath).toString(), "msg", startLine, startColumn, endLine, endColumn);
  }

}
