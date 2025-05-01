/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.sarif;

import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.HashSet;
import java.util.AbstractMap.SimpleEntry;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.batch.fs.TextRange;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.Severity;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.batch.sensor.issue.ExternalIssue;
import org.sonar.api.batch.sensor.issue.Issue;
import org.sonar.api.batch.sensor.issue.Issue.Flow;
import org.sonar.api.batch.sensor.issue.IssueLocation;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.batch.sensor.rule.AdHocRule;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.issue.impact.SoftwareQuality;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.RuleType;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;
import org.sonar.api.utils.Version;
import org.sonarsource.dotnet.shared.plugins.SarifParserCallbackImpl;

import static java.util.Arrays.asList;
import static java.util.Collections.emptyList;
import static java.util.Collections.emptySet;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.groups.Tuple.tuple;
import static org.junit.Assert.assertThrows;

public class SarifParserCallbackImplTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  private SensorContextTester ctx;
  private final Map<String, String> repositoryKeyByRoslynRuleKey = new HashMap<>();

  private SarifParserCallbackImpl callback;

  @Before
  public void setUp() {
    logTester.setLevel(Level.DEBUG);
    ctx = SensorContextTester.create(temp.getRoot().toPath());
    repositoryKeyByRoslynRuleKey.put("rule1", "rule1");
    repositoryKeyByRoslynRuleKey.put("rule2", "rule2");
    repositoryKeyByRoslynRuleKey.put("rule42", "csharpsquid");

    // file needs to have a few lines so that the issue is within it's range
    ctx.fileSystem().add(TestInputFileBuilder.create("module1", "file1")
      .setContents("My file\ncontents\nwith some\n lines")
      .build());

    ctx.fileSystem().add(TestInputFileBuilder.create("module1", "Dummy.razor")
      .setContents("My file\ncontents\nwith some\n lines")
      .build());

    ctx.fileSystem().add(TestInputFileBuilder.create("module1", "Dummy.cshtml")
      .setContents("My file\ncontents\nwith some\n lines")
      .build());

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, true, emptySet(), emptySet(), emptySet());
  }

  @Test
  public void should_add_project_issues() {
    callback.onProjectIssue("rule1", "warning", ctx.project(), "msg");
    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues().iterator().next().primaryLocation().inputComponent().key()).isEqualTo("projectKey");
    assertThat(ctx.allIssues().iterator().next().ruleKey().rule()).isEqualTo("rule1");
    assertThat(logTester.logs(Level.DEBUG)).containsOnly("Adding project level issue rule1: [key=projectKey]");
  }

  @Test
  public void should_add_file_issues_no_secondary_location() {
    String absoluteFilePath = temp.getRoot().toPath().resolve("file1").toString();
    callback.onFileIssue("rule1", "warning", absoluteFilePath, emptyList(), "msg");
    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues().iterator().next().primaryLocation().inputComponent().key()).isEqualTo("module1:file1");
    assertThat(ctx.allIssues().iterator().next().ruleKey().rule()).isEqualTo("rule1");
    assertThat(logTester.logs(Level.DEBUG)).containsOnly("Adding file level issue rule1: file1");
    assertThat(ctx.allIssues().iterator().next().flows()).isEmpty();
  }

  @Test
  public void should_add_file_issues_with_secondary_location() {
    String absoluteFilePath = temp.getRoot().toPath().resolve("file1").toString();
    callback.onFileIssue("rule1", "warning", absoluteFilePath, Collections.singletonList(createLocation("file1", 4, 5)), "msg");
    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues().iterator().next().primaryLocation().inputComponent().key()).isEqualTo("module1:file1");
    assertThat(ctx.allIssues().iterator().next().ruleKey().rule()).isEqualTo("rule1");
    assertThat(logTester.logs(Level.DEBUG)).containsOnly("Adding file level issue rule1: file1");

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
  public void should_create_external_file_issue_for_unknown_rule_key() {
    String absoluteFilePath = temp.getRoot().toPath().resolve("file1").toString();
    callback.onFileIssue("rule45", "warning", absoluteFilePath, emptyList(), "msg");
    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onFileIssue("rule45", "warning", absoluteFilePath, emptyList(), "msg");
    callback.onFileIssue("S1234", "warning", absoluteFilePath, emptyList(), "msg"); // sonar rule, ignored

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, ExternalIssue::type, ExternalIssue::severity)
      .containsExactlyInAnyOrder(
        tuple("rule45", RuleType.CODE_SMELL, Severity.MAJOR));

    assertThat(logTester.logs(Level.DEBUG)).containsOnly("Adding file level external issue rule45: file1");
  }

  @Test
  public void should_create_external_file_issue_with_secondary_location() {
    String absoluteFilePath = temp.getRoot().toPath().resolve("file1").toString();
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    callback.onFileIssue("rule45", "warning", absoluteFilePath, Collections.singletonList(createLocation("file1", 4, 5)), "msg");

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, ExternalIssue::type, ExternalIssue::severity)
      .containsExactlyInAnyOrder(
        tuple("rule45", RuleType.CODE_SMELL, Severity.MAJOR));
    List<Flow> flows = ctx.allExternalIssues().iterator().next().flows();
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
  public void should_ignore_file_issue_with_unknown_file() {
    callback.onFileIssue("rule1", "warning", "file-unknown", emptyList(), "msg");
    assertThat(ctx.allIssues()).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsOnly("Skipping issue rule1, input file not found or excluded: file-unknown");
  }

  @Test
  public void should_ignore_issue_with_unknown_file() {
    callback.onIssue("rule1", "warning", createLocation("file-unknown", 2, 3), Collections.emptyList(), false);
    assertThat(ctx.allIssues()).isEmpty();
    assertThat(logTester.logs(Level.DEBUG))
      .containsOnly(String.format("Skipping issue rule1, input file not found or excluded: " + createAbsolutePath("file-unknown")));
  }

  @Test
  public void should_ignore_project_issue_with_unknown_rule_key() {
    callback.onProjectIssue("rule45", "warning", ctx.project(), "msg");
    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, true, emptySet(), emptySet(), emptySet());

    callback.onProjectIssue("rule45", "warning", ctx.project(), "msg");

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();
  }

  @Test
  public void should_add_issues() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("rule2", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);

    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1", "rule2");
    assertThat(logTester.logs(Level.DEBUG)).containsOnly(
      "Adding normal issue rule1: " + createAbsolutePath("file1"),
      "Adding normal issue rule2: " + createAbsolutePath("file1")
    );
  }

  @Test
  public void should_add_execution_flow() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.singletonList(createLocation("file1", 4, 5)), true);

    assertThat(ctx.allIssues())
      .extracting(Issue::flows)
      .hasSize(1)
      .extracting(x -> x.get(0))
      .extracting(
        Flow::type,
        Flow::description,
        x -> x.locations().get(0).inputComponent().key(),
        x -> x.locations().get(0).textRange().start().line(),
        x -> x.locations().get(0).textRange().start().lineOffset(),
        x -> x.locations().get(0).textRange().end().line(),
        x -> x.locations().get(0).textRange().end().lineOffset())
      .containsOnly(tuple(NewIssue.FlowType.EXECUTION, "Execution Flow", "module1:file1", 4, 5, 4, 6));
  }

  @Test
  public void should_not_add_execution_flow_with_no_secondary_locations() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList(), true);

    assertThat(ctx.allIssues())
      .extracting(Issue::flows)
      .allSatisfy(flows -> assertThat(flows).isEmpty());
  }

  @Test
  public void should_not_add_execution_flow_with_secondary_locations_invalid_file() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.singletonList(createLocation("does-not-exit-file", 4, 5)), true);

    assertThat(ctx.allIssues())
      .hasSize(1)
      .extracting(Issue::flows)
      .allSatisfy(flows -> assertThat(flows).isEmpty());
  }

  @Test
  public void should_create_external_issue_for_unknown_rule_key() {
    callback.onIssue("rule45", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues()).isEmpty();

    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    callback.onIssue("rule45", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("S1234", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false); // sonar rule, ignored

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, ExternalIssue::type, ExternalIssue::severity)
      .containsExactlyInAnyOrder(
        tuple("rule45", RuleType.CODE_SMELL, Severity.MAJOR));
    assertThat(logTester.logs(Level.DEBUG)).containsOnly("Adding external issue rule45: " + createAbsolutePath("file1"));
  }

  @Test
  public void should_create_external_issues_with_correct_impact_mapping() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    callback.onIssue("rule45", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("rule46", "error", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("rule47", "info", createLocation("file1", 2, 3), Collections.emptyList(), false);

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, x -> x.impacts().entrySet().toArray()[0])
      .containsExactlyInAnyOrder(
        tuple("rule45", new SimpleEntry<>(SoftwareQuality.MAINTAINABILITY, org.sonar.api.issue.impact.Severity.MEDIUM)),
        tuple("rule46", new SimpleEntry<>(SoftwareQuality.MAINTAINABILITY, org.sonar.api.issue.impact.Severity.HIGH)),
        tuple("rule47", new SimpleEntry<>(SoftwareQuality.MAINTAINABILITY, org.sonar.api.issue.impact.Severity.INFO))
        );
    assertThat(logTester.logs(Level.DEBUG)).containsExactlyInAnyOrder(
      "Adding external issue rule45: " + createAbsolutePath("file1"),
      "Adding external issue rule46: " + createAbsolutePath("file1"),
      "Adding external issue rule47: " + createAbsolutePath("file1")
    );
  }

  @Test
  public void should_create_external_issues_without_impact_cloud() {
    SensorContextTester ctxCloud = SensorContextTester.create(temp.getRoot().toPath());
    ctxCloud.setRuntime(SonarRuntimeImpl.forSonarQube(Version.create(10, 1), SonarQubeSide.SCANNER, SonarEdition.SONARCLOUD));
    ctxCloud.fileSystem().add(TestInputFileBuilder.create("module1", "file1")
      .setContents("My file\ncontents\nwith some\n lines")
      .build());
    callback = new SarifParserCallbackImpl(ctxCloud, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    callback.onIssue("rule45", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("rule46", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("rule47", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    
    assertThat(ctxCloud.allExternalIssues())
      .extracting(ExternalIssue::ruleId, x -> x.impacts().keySet())
      .containsExactlyInAnyOrder(
        tuple("rule45", Collections.emptySet()),
        tuple("rule46", Collections.emptySet()),
        tuple("rule47", Collections.emptySet())
      );
  }

  @Test
  public void external_issue_with_invalid_precise_location_reports_on_line() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    // We try to report an issue that contains an invalid startColumn (bigger than the line length) but with a valid start line
    // So we expect the issue to be reported on the start line.
    callback.onIssue("rule45", "warning", createLocation("file1", 2, 20, 4, 2), Collections.emptyList(), false);

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
    callback.onIssue("rule45", "warning", createLocation("file1", 10, 12), Collections.emptyList(), false);

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
    callback.onIssue("rule45", "warning", createLocation("file1", 10, 53, 80, 42), Collections.emptyList(), false);

    assertThat(ctx.allIssues()).isEmpty();
    assertThat(ctx.allExternalIssues())
      .extracting(ExternalIssue::ruleId, i -> i.primaryLocation().inputComponent().key(), i -> i.primaryLocation().textRange())
      .containsExactlyInAnyOrder(
        tuple("rule45", "module1:file1", null));
  }

  @Test
  public void should_add_issue_with_secondary_location() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.singletonList(createLocation("file1", 4, 5)), false);

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
  public void should_add_issue_with_secondary_location_with_invalid_file() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.singletonList(createLocation("invalid-file", 4, 5)), false);

    assertThat(ctx.allIssues())
      .hasSize(1)
      .extracting(Issue::flows)
      .allSatisfy(flows -> assertThat(flows).isEmpty());
  }

  @Test
  public void should_ignore_repeated_module_issues() {
    callback.onProjectIssue("rule1", "warning", ctx.project(), "message");
    callback.onProjectIssue("rule1", "warning", ctx.project(), "message");

    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1");
  }

  @Test
  public void should_ignore_repeated_file_issues() {
    callback.onFileIssue("rule1", "warning", createAbsolutePath("file1"), emptyList(), "message");
    callback.onFileIssue("rule1", "warning", createAbsolutePath("file1"), emptyList(), "message");

    assertThat(ctx.allIssues()).hasSize(1);
    assertThat(ctx.allIssues()).extracting("ruleKey").extracting("rule")
      .containsOnly("rule1");
  }

  @Test
  public void should_ignore_repeated_issues() {
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);
    callback.onIssue("rule1", "warning", createLocation("file1", 2, 3), Collections.emptyList(), false);

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

    callback.onIssue("rule45", null, createLocation("file1", 2, 3), Collections.emptyList(), false);

    assertThat(ctx.allExternalIssues()).extracting(ExternalIssue::severity).containsExactly(Severity.INFO);
  }

  @Test
  public void should_fallback_on_major_severity() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    callback.onIssue("rule45", null, createLocation("file1", 2, 3), Collections.emptyList(), false);

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

  @Test
  public void issue_with_invalid_precise_location_forRazor_reports_on_line() {
    assertIssueReportedOnLine("Dummy.razor");
  }

  @Test
  public void issue_with_invalid_precise_location_forCshtml_reports_on_line() {
    assertIssueReportedOnLine("Dummy.cshtml");
  }

  @Test
  public void project_level_issues_for_different_projects() {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());
    repositoryKeyByRoslynRuleKey.put("S3990", "S3990");

    callback.onProjectIssue("S3990", "level", ctx.project(), "Provide a 'CLSCompliant' attribute for assembly 'First'.");
    callback.onProjectIssue("S3990", "level", ctx.project(), "Provide a 'CLSCompliant' attribute for assembly 'First'.");
    assertThat(ctx.allIssues()).hasSize(1); // Issues with the same id and message are considered duplicates.

    callback.onProjectIssue("S3990", "level", ctx.project(), "Provide a 'CLSCompliant' attribute for assembly 'Second'.");
    assertThat(ctx.allIssues()).hasSize(2); // A different message leads to a different issue
  }

  @Test
  public void impact_severity_mapping_is_correct() {
    assertThat(SarifParserCallbackImpl.mapImpactSeverity(Severity.BLOCKER)).isEqualTo(org.sonar.api.issue.impact.Severity.BLOCKER);
    assertThat(SarifParserCallbackImpl.mapImpactSeverity(Severity.CRITICAL)).isEqualTo(org.sonar.api.issue.impact.Severity.HIGH);
    assertThat(SarifParserCallbackImpl.mapImpactSeverity(Severity.MAJOR)).isEqualTo(org.sonar.api.issue.impact.Severity.MEDIUM);
    assertThat(SarifParserCallbackImpl.mapImpactSeverity(Severity.MINOR)).isEqualTo(org.sonar.api.issue.impact.Severity.LOW);
    assertThat(SarifParserCallbackImpl.mapImpactSeverity(Severity.INFO)).isEqualTo(org.sonar.api.issue.impact.Severity.INFO);
  }

  @Test
  public void impact_softwareQuality_mapping_is_correct() {
    assertThat(SarifParserCallbackImpl.mapSoftwareQuality(RuleType.CODE_SMELL)).isEqualTo(SoftwareQuality.MAINTAINABILITY);
    assertThat(SarifParserCallbackImpl.mapSoftwareQuality(RuleType.BUG)).isEqualTo(SoftwareQuality.RELIABILITY);
    assertThat(SarifParserCallbackImpl.mapSoftwareQuality(RuleType.VULNERABILITY)).isEqualTo(SoftwareQuality.SECURITY);
    assertThrows(IllegalStateException.class, () -> SarifParserCallbackImpl.mapSoftwareQuality(RuleType.SECURITY_HOTSPOT));
  }

  private void assertIssueReportedOnLine(String fileName) {
    callback = new SarifParserCallbackImpl(ctx, repositoryKeyByRoslynRuleKey, false, emptySet(), emptySet(), emptySet());

    // We try to report an issue that contains an invalid startColumn (bigger than the line length) but with a valid start line
    // So we expect the issue to be reported on the start line.
    callback.onIssue("rule42", "warning", createLocation(fileName, 2, 99, 2, 101), Collections.emptyList(), false);

    assertThat(ctx.allIssues()).hasSize(1)
      .extracting(Issue::ruleKey,
        i -> i.primaryLocation().textRange().start().line(),
        i -> i.primaryLocation().textRange().start().lineOffset(),
        i -> i.primaryLocation().textRange().end().line(),
        i -> i.primaryLocation().textRange().end().lineOffset())
      .containsExactlyInAnyOrder(
        tuple(RuleKey.of("csharpsquid", "rule42"), 2, 0, 2, 8));

    List<String> logs = logTester.logs();
    assertThat(logs.get(1))
      .startsWith("Precise issue location cannot be found! Location:")
      .endsWith(fileName + ", message=msg, startLine=2, startColumn=99, endLine=2, endColumn=101]");
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
