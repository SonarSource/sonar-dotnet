/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.TextRange;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.Severity;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.batch.sensor.issue.ExternalIssue;
import org.sonar.api.batch.sensor.issue.Issue.Flow;
import org.sonar.api.batch.sensor.issue.IssueLocation;
import org.sonar.api.batch.sensor.rule.AdHocRule;
import org.sonar.api.rules.RuleType;
import org.sonarsource.dotnet.shared.plugins.SarifParserCallbackImpl;

import static java.util.Arrays.asList;
import static java.util.Collections.emptySet;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.groups.Tuple.tuple;

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

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, true, emptySet(), emptySet(), emptySet());
  }

  @Test
  public void should_add_project_issues() {
    callback.onProjectIssue("rule1", "warning", ctx.module(), "msg");
    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues().iterator().next().primaryLocation().inputComponent().key()).isEqualTo("projectKey");
    assertThat(ctx.allIssues().iterator().next().ruleKey().rule()).isEqualTo("rule1");
  }

  @Test
  public void should_add_file_issues() {
    String absoluteFilePath = temp.getRoot().toPath().resolve("file1").toString();
    callback.onFileIssue("rule1", "warning", absoluteFilePath, "msg");
    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues().iterator().next().primaryLocation().inputComponent().key()).isEqualTo("module1:file1");
    assertThat(ctx.allIssues().iterator().next().ruleKey().rule()).isEqualTo("rule1");
  }

  @Test
  public void should_create_external_file_issue_for_unknown_rule_key() {
    String absoluteFilePath = temp.getRoot().toPath().resolve("file1").toString();
    callback.onFileIssue("rule45", "warning", absoluteFilePath, "msg");
    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onFileIssue("rule45", "warning", absoluteFilePath, "msg");
    callback.onFileIssue("S1234", "warning", absoluteFilePath, "msg"); // sonar rule, ignored

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, ExternalIssue::type, ExternalIssue::severity)
      .containsExactlyInAnyOrder(
        tuple("rule45", RuleType.CODE_SMELL, Severity.MAJOR));
  }

  @Test
  public void should_ignore_file_issue_with_unknown_file() {
    callback.onFileIssue("rule1", "warning", "file-unknown", "msg");
    assertThat(ctx.allIssues()).isEmpty();
  }

  @Test
  public void should_ignore_project_issue_with_unknown_rule_key() {
    callback.onProjectIssue("rule45", "warning", ctx.module(), "msg");
    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, true, emptySet(), emptySet(), emptySet());

    callback.onProjectIssue("rule45", "warning", ctx.module(), "msg");

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();
  }

  @Test
  public void should_add_issues() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList());
    callback.onIssue("rule2", "warning", createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1", "rule2");
  }

  @Test
  public void should_create_external_issue_for_unknown_rule_key() {
    callback.onIssue("rule45", "warning", createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    callback.onIssue("rule45", "warning", createLocation("file1", 2, 3), Collections.emptyList());
    callback.onIssue("S1234", "warning", createLocation("file1", 2, 3), Collections.emptyList()); // sonar rule, ignored

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, ExternalIssue::type, ExternalIssue::severity)
      .containsExactlyInAnyOrder(
        tuple("rule45", RuleType.CODE_SMELL, Severity.MAJOR));
  }

  @Test
  public void external_issue_with_invalid_precise_location_reports_on_line() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    // We try to report an issue that contains an invalid startColumn (bigger than the line length) but with a valid start line
    // So we expect the issue to be reported on the start line.
    callback.onIssue("rule45", "warning", createLocation("file1", 2, 20, 4, 2), Collections.emptyList());

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId,
        i -> i.primaryLocation().textRange().start().line(),
        i -> i.primaryLocation().textRange().start().lineOffset(),
        i -> i.primaryLocation().textRange().end().line(),
        i -> i.primaryLocation().textRange().end().lineOffset())
      .containsExactlyInAnyOrder(
        tuple("rule45", 2, 0, 2, 8));
  }

  @Test
  public void external_issue_with_invalid_line_location_reports_on_file() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    // We try to report an issue that contains an invalid startLine (bigger than the file length)
    // So we expect the issue to be reported on the file.
    callback.onIssue("rule45", "warning", createLocation("file1", 10, 12), Collections.emptyList());

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, i -> i.primaryLocation().inputComponent().key(), i -> i.primaryLocation().textRange())
      .containsExactlyInAnyOrder(
        tuple("rule45", "module1:file1", null));
  }

  @Test
  public void external_issue_with_invalid_precise_location_reports_on_file() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    // We try to report an issue that contains an invalid startLine (bigger than the file length)
    // So we expect the issue to be reported on the file.
    callback.onIssue("rule45", "warning", createLocation("file1", 10, 53, 80, 42), Collections.emptyList());

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, i -> i.primaryLocation().inputComponent().key(), i -> i.primaryLocation().textRange())
      .containsExactlyInAnyOrder(
        tuple("rule45", "module1:file1", null));
  }

  @Test
  public void should_add_issue_with_secondary_location() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.singletonList(createLocation("file1", 4, 5)));

    assertThat(ctx.allIssues()).hasSize(1);

    List<Flow> flows = ctx.allIssues().iterator().next().flows();
    assertThat(flows).hasSize(1);

    List<IssueLocation> locations = flows.get(0).locations();
    assertThat(locations).hasSize(1);

    assertThat(locations.get(0).inputComponent().key()).isEqualTo("module1:file1");
    TextRange textRange = locations.get(0).textRange();
    assertThat(textRange.start().lineOffset()).isEqualTo(5);
    assertThat(textRange.start().line()).isEqualTo(4);
    assertThat(textRange.end().lineOffset()).isEqualTo(6);
    assertThat(textRange.end().line()).isEqualTo(4);
  }

  @Test
  public void should_ignore_repeated_module_issues() {
    callback.onProjectIssue("rule1", "warning", ctx.module(), "message");
    callback.onProjectIssue("rule1", "warning", ctx.module(), "message");

    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1");
  }

  @Test
  public void should_ignore_repeated_file_issues() {
    callback.onFileIssue("rule1", "warning", createAbsolutePath("file1"), "message");
    callback.onFileIssue("rule1", "warning", createAbsolutePath("file1"), "message");

    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1");
  }

  @Test
  public void should_ignore_repeated_issues() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList());
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1");
  }

  @Test
  public void should_register_adhoc_rule() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onRule("rule123", "My rule", "Rule description", "Error", "Foo");

    assertThat(ctx.allAdHocRules())
      .extracting(AdHocRule::engineId, AdHocRule::ruleId, AdHocRule::name, AdHocRule::description, AdHocRule::severity, AdHocRule::type)
      .containsExactlyInAnyOrder(tuple("roslyn", "rule123", "My rule", "Rule description", Severity.CRITICAL, RuleType.BUG));
  }

  @Test
  public void should_ignore_adhoc_rule_matching_sonar_key() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onRule("SS1234", "My rule", "Rule description", "Error", "Foo"); // does not start with single S
    callback.onRule("S123456", "My rule", "Rule description", "Error", "Foo"); // too long
    callback.onRule("S12345", "My rule", "Rule description", "Error", "Foo"); // too long
    callback.onRule("S1234", "My rule", "Rule description", "Error", "Foo"); // sonar rule
    callback.onRule("S123", "My rule", "Rule description", "Error", "Foo"); // sonar rule
    callback.onRule("S12", "My rule", "Rule description", "Error", "Foo"); // too short
    callback.onRule("S123x", "My rule", "Rule description", "Error", "Foo"); // does not have only digits after S

    assertThat(ctx.allAdHocRules())
      .extracting(AdHocRule::engineId, AdHocRule::ruleId, AdHocRule::name, AdHocRule::description, AdHocRule::severity, AdHocRule::type)
      .containsExactlyInAnyOrder(
        tuple("roslyn", "SS1234", "My rule", "Rule description", Severity.CRITICAL, RuleType.BUG),
        tuple("roslyn", "S123456", "My rule", "Rule description", Severity.CRITICAL, RuleType.BUG),
        tuple("roslyn", "S12345", "My rule", "Rule description", Severity.CRITICAL, RuleType.BUG),
        tuple("roslyn", "S12", "My rule", "Rule description", Severity.CRITICAL, RuleType.BUG),
        tuple("roslyn", "S123x", "My rule", "Rule description", Severity.CRITICAL, RuleType.BUG));
  }

  @Test
  public void should_map_severity_name_and_description() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onRule("S1", "My rule1", "Rule description", "Error", "Foo");
    callback.onRule("S2", null, null, "Warning", "Foo");
    callback.onRule("S3", "My rule3", null, "Info", "Foo");
    callback.onRule("S4", null, "Rule description", "Note", "Foo");

    assertThat(ctx.allAdHocRules())
      .extracting(AdHocRule::ruleId, AdHocRule::severity, AdHocRule::name, AdHocRule::description)
      .containsExactlyInAnyOrder(
        tuple("S1", Severity.CRITICAL, "My rule1", "Rule description"),
        tuple("S2", Severity.MAJOR, "S2", null),
        tuple("S3", Severity.INFO, "My rule3", null),
        tuple("S4", Severity.INFO, "S4", "Rule description"));
  }

  @Test
  public void should_fallback_on_rule_severity() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onRule("rule45", "My rule", "Rule description", "Note", "Foo");

    callback.onIssue("rule45", null, createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allExternalIssues()).extracting(ExternalIssue::severity).containsExactly(Severity.INFO);
  }

  @Test
  public void should_fallback_on_major_severity() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    callback.onIssue("rule45", null, createLocation("file1", 2, 3), Collections.emptyList());

    assertThat(ctx.allExternalIssues()).extracting(ExternalIssue::severity).containsExactly(Severity.MAJOR);
  }

  @Test
  public void should_map_rule_type() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, new HashSet<>(asList("bug1", "bug2")), new HashSet<>(asList("cs1", "cs2")),
      new HashSet<>(asList("vul1", "vul2")));

    callback.onRule("S1", "My rule", "Rule description", "Info", "bug1");
    callback.onRule("S2", "My rule", "Rule description", "Info", "cs2");
    callback.onRule("S3", "My rule", "Rule description", "Info", "vul1");
    callback.onRule("S4", "My rule", "Rule description", "Info", "unknown");
    callback.onRule("S5", "My rule", "Rule description", "Info", null);

    assertThat(ctx.allAdHocRules())
      .extracting(AdHocRule::ruleId, AdHocRule::type)
      .containsExactlyInAnyOrder(
        tuple("S1", RuleType.BUG),
        tuple("S2", RuleType.CODE_SMELL),
        tuple("S3", RuleType.VULNERABILITY),
        tuple("S4", RuleType.CODE_SMELL),
        tuple("S5", RuleType.CODE_SMELL));
  }

  private Location createLocation(String filePath, int line, int column) {
    return createLocation(filePath, line, column, line, column + 1);
  }

  private Location createLocation(String filePath, int startLine, int startColumn, int endLine, int endColumn) {
    return new Location(createAbsolutePath(filePath), "msg", startLine, startColumn, endLine, endColumn);
  }

  private String createAbsolutePath(String filePath) {
    return temp.getRoot().toPath().resolve(filePath).toString();
  }
}
