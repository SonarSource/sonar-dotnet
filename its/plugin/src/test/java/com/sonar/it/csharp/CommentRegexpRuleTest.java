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

import com.google.common.collect.ImmutableMap;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.SonarRunner;
import org.junit.Assume;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Test;
import org.sonar.wsclient.SonarClient;
import org.sonar.wsclient.issue.Issue;
import org.sonar.wsclient.issue.IssueQuery;

import java.io.File;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static org.fest.assertions.Assertions.assertThat;

public class CommentRegexpRuleTest {

  private static final String PROJECT = "CommentRegexpRuleTest";
  private static final String PROFILE_NAME = "template_rule";
  private static final String REPOSITORY_KEY = "csharpsquid";
  private static final String TEMPLATE_RULE_KEY = "S124";
  private static final String RULE_KEY = "Foo";

  @ClassRule
  public static Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void resetData() throws Exception {
    orchestrator.resetData();
  }

  @Test
  public void test() {
    Assume.assumeTrue(Tests.is_at_least_plugin_4_1());

    SonarClient sonarClient = orchestrator.getServer().adminWsClient();
    sonarClient.post("/api/rules/create", ImmutableMap.<String, Object>builder()
      .put("name", PROJECT)
      .put("markdown_description", PROJECT)
      .put("severity", "INFO")
      .put("status", "READY")
      .put("template_key", REPOSITORY_KEY + ":" + TEMPLATE_RULE_KEY)
      .put("custom_key", RULE_KEY)
      .put("prevent_reactivation", "true")
      .put("params", "message=\"message1\";regularExpression=\"foo\"")
      .build());
    String profiles = sonarClient.get("api/rules/app");
    Pattern pattern = Pattern.compile("cs-" + PROFILE_NAME + "-\\d+");
    Matcher matcher = pattern.matcher(profiles);
    assertThat(matcher.find());
    String profilekey = matcher.group();
    sonarClient.post("api/qualityprofiles/activate_rule", ImmutableMap.<String, Object>of(
      "profile_key", profilekey,
      "rule_key", REPOSITORY_KEY + ":" + RULE_KEY,
      "severity", "INFO",
      "params", ""));

    SonarRunner build = Tests.createSonarRunnerBuild()
      .setProjectDir(new File("projects/" + PROJECT))
      .setProjectKey(PROJECT)
      .setProjectName(PROJECT)
      .setProjectVersion("1.0")
      .setSourceDirs(".")
      .setProfile(PROFILE_NAME);
    orchestrator.executeBuild(build);

    List<Issue> issues = getIssues(REPOSITORY_KEY + ":" + RULE_KEY);
    assertThat(issues.size()).isEqualTo(1);
    Issue issue = issues.get(0);
    assertThat(issue.line()).isEqualTo(3);
  }

  private List<Issue> getIssues(String ruleKey) {
    IssueQuery query = IssueQuery.create().componentRoots(PROJECT).rules(ruleKey);
    return orchestrator.getServer().wsClient().issueClient().find(query).list();
  }

}
