/*
 * SonarQube C# Plugin
 * Copyright (C) 2014 SonarSource
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
package org.sonar.plugins.csharp;

import com.google.common.base.Charsets;
import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableSet;
import com.google.common.io.Files;
import org.junit.Before;
import org.junit.Test;
import org.mockito.ArgumentCaptor;
import org.mockito.Mockito;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.component.ResourcePerspectives;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.Issuable;
import org.sonar.api.issue.Issuable.IssueBuilder;
import org.sonar.api.issue.Issue;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.measures.Measure;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CSharpSensorTest {

  private SensorContext context;
  private Project project;
  private CSharpSensor sensor;
  private Settings settings;
  private DefaultInputFile inputFile;
  private DefaultFileSystem fs;
  private FileLinesContext fileLinesContext;
  private FileLinesContextFactory fileLinesContextFactory;
  private RuleRunnerExtractor extractor;
  private NoSonarFilter noSonarFilter;
  private ResourcePerspectives perspectives;
  private Issuable issuable;
  private IssueBuilder issueBuilder1;
  private IssueBuilder issueBuilder2;
  private IssueBuilder issueBuilder3;
  private Issue issue1;
  private Issue issue2;
  private Issue issue3;

  @Test
  public void shouldExecuteOnProject() {
    DefaultFileSystem fs = new DefaultFileSystem();

    CSharpSensor sensor =
      new CSharpSensor(
        mock(Settings.class), mock(RuleRunnerExtractor.class),
        fs,
        mock(FileLinesContextFactory.class), mock(NoSonarFilter.class), mock(RulesProfile.class), mock(ResourcePerspectives.class));

    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("foo").setAbsolutePath("foo").setLanguage("java"));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("bar").setAbsolutePath("bar").setLanguage("cs").setType(Type.TEST));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("baz").setAbsolutePath("baz").setLanguage("cs"));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isTrue();
  }

  @Before
  public void init() {
    fs = new DefaultFileSystem();
    fs.setWorkDir(new File("src/test/resources/CSharpSensorTest"));

    inputFile = new DefaultInputFile("Foo&Bar.cs").setAbsolutePath("Foo&Bar.cs").setLanguage("cs");
    fs.add(inputFile);

    fileLinesContext = mock(FileLinesContext.class);
    fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(inputFile)).thenReturn(fileLinesContext);

    extractor = mock(RuleRunnerExtractor.class);
    when(extractor.executableFile()).thenReturn(new File("src/test/resources/CSharpSensorTest/fake.bat"));

    noSonarFilter = mock(NoSonarFilter.class);
    perspectives = mock(ResourcePerspectives.class);
    issuable = mock(Issuable.class);
    issueBuilder1 = mock(IssueBuilder.class);
    issueBuilder2 = mock(IssueBuilder.class);
    issueBuilder3 = mock(IssueBuilder.class);
    when(issuable.newIssueBuilder()).thenReturn(issueBuilder1, issueBuilder2, issueBuilder3);
    issue1 = mock(Issue.class);
    when(issueBuilder1.build()).thenReturn(issue1);
    issue2 = mock(Issue.class);
    when(issueBuilder2.build()).thenReturn(issue2);
    issue3 = mock(Issue.class);
    when(issueBuilder3.build()).thenReturn(issue3);
    when(perspectives.as(Mockito.eq(Issuable.class), Mockito.any(InputFile.class))).thenReturn(issuable);

    ActiveRule templateActiveRule = mock(ActiveRule.class);
    when(templateActiveRule.getRuleKey()).thenReturn("[template_key\"'<>&]");
    Rule templateRule = mock(Rule.class);
    Rule baseTemplateRule = mock(Rule.class);
    when(baseTemplateRule.getKey()).thenReturn("[base_key]");
    when(templateRule.getTemplate()).thenReturn(baseTemplateRule);
    when(templateActiveRule.getRule()).thenReturn(templateRule);

    ActiveRule parametersActiveRule = mock(ActiveRule.class);
    when(parametersActiveRule.getRuleKey()).thenReturn("[parameters_key]");
    ActiveRuleParam param1 = mock(ActiveRuleParam.class);
    when(param1.getKey()).thenReturn("[param1_key]");
    when(param1.getValue()).thenReturn("[param1_value]");
    when(parametersActiveRule.getActiveRuleParams()).thenReturn(ImmutableList.of(param1));
    Rule parametersRule = mock(Rule.class);
    RuleParam param1Default = mock(org.sonar.api.rules.RuleParam.class);
    when(param1Default.getKey()).thenReturn("[param1_key]");
    when(param1Default.getDefaultValue()).thenReturn("[param1_default_value]");
    RuleParam param2Default = mock(org.sonar.api.rules.RuleParam.class);
    when(param2Default.getKey()).thenReturn("[param2_default_key]");
    when(param2Default.getDefaultValue()).thenReturn("[param2_default_value]");
    when(parametersRule.getParams()).thenReturn(ImmutableList.of(param1Default, param2Default));
    when(parametersActiveRule.getRule()).thenReturn(parametersRule);

    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository("csharpsquid")).thenReturn(ImmutableList.of(templateActiveRule, parametersActiveRule));

    settings = mock(Settings.class);
    sensor =
      new CSharpSensor(
        settings, extractor,
        fs,
        fileLinesContextFactory, noSonarFilter, rulesProfile, perspectives);

    project = mock(Project.class);
    context = mock(SensorContext.class);
  }

  @Test
  public void metrics() {
    sensor.analyse(project, context);

    verify(context).saveMeasure(inputFile, CoreMetrics.LINES, 27d);
    verify(context).saveMeasure(inputFile, CoreMetrics.CLASSES, 1d);
    verify(context).saveMeasure(inputFile, CoreMetrics.ACCESSORS, 5d);
    verify(context).saveMeasure(inputFile, CoreMetrics.STATEMENTS, 2d);
    verify(context).saveMeasure(inputFile, CoreMetrics.FUNCTIONS, 3d);
    verify(context).saveMeasure(inputFile, CoreMetrics.PUBLIC_API, 4d);
    verify(context).saveMeasure(inputFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API, 2d);
    verify(context).saveMeasure(inputFile, CoreMetrics.COMPLEXITY, 3d);
  }

  @Test
  public void distribution() {
    sensor.analyse(project, context);

    ArgumentCaptor<Measure> captor = ArgumentCaptor.forClass(Measure.class);
    verify(context, Mockito.times(2)).saveMeasure(Mockito.eq(inputFile), captor.capture());
    int i = 0;
    for (Measure measure : captor.getAllValues()) {
      if (measure.getMetric().equals(CoreMetrics.FILE_COMPLEXITY_DISTRIBUTION)) {
        i++;
        assertThat(measure.getData()).isEqualTo("0=1;5=0;10=0;20=0;30=0;60=0;90=0");
      } else if (measure.getMetric().equals(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION)) {
        i++;
        assertThat(measure.getData()).isEqualTo("1=3;2=0;4=0;6=0;8=0;10=0;12=0");
      }
    }
    assertThat(i).isEqualTo(2);
  }

  @Test
  public void commentsAndNoSonar() {
    sensor.analyse(project, context);

    verify(noSonarFilter).addComponent(inputFile.key(), ImmutableSet.of(8));
    verify(context).saveMeasure(inputFile, CoreMetrics.COMMENT_LINES, 2d);
  }

  @Test
  public void devCockpit() {
    sensor.analyse(project, context);

    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 3, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 7, 1);

    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 1, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 12, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 13, 1);
  }

  @Test
  public void issue() {
    sensor.analyse(project, context);

    verify(issueBuilder1).ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S1186"));
    verify(issueBuilder1).message("Add a nested comment explaining why this method is empty, throw an NotSupportedException or complete the implementation.");
    verify(issueBuilder1).line(16);
    verify(issuable).addIssue(issue1);
  }

  @Test
  public void escapesAnalysisInput() throws Exception {
    sensor.analyse(project, context);

    assertThat(
      Files.toString(new File("src/test/resources/CSharpSensorTest/analysis-input.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", "")
        .replaceAll("<File>.*?Foo&amp;Bar.cs</File>", "<File>Foo&amp;Bar.cs</File>"))
      .isEqualTo(Files.toString(new File("src/test/resources/CSharpSensorTest/analysis-input-expected.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", ""));
  }

  @Test
  public void roslynReportIsProcessed() {
    when(settings.getString("sonar.cs.roslyn.reportFilePath")).thenReturn(new File("src/test/resources/CSharpSensorTest/roslyn-report.json").getAbsolutePath());
    sensor.analyse(project, context);

    // We use a mocked rule runner which will report an issue even if a Roslyn report is provided
    verify(issueBuilder1).ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S1186"));
    verify(issueBuilder1).message("Add a nested comment explaining why this method is empty, throw an NotSupportedException or complete the implementation.");
    verify(issueBuilder1).line(16);

    verify(issueBuilder2).ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "[parameters_key]"));
    verify(issueBuilder2).message("Short messages should be used first in Roslyn reports");
    verify(issueBuilder2).line(43);

    verify(issueBuilder3).ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "[parameters_key]"));
    verify(issueBuilder3).message("There only is a full message in the Roslyn report");
    verify(issueBuilder3).line(1);

    verify(issuable).addIssue(issue1);
    verify(issuable).addIssue(issue2);
    verify(issuable).addIssue(issue3);
  }

  @Test
  public void roslynRulesNotExecutedTwice() throws Exception {
    when(settings.getString("sonar.cs.roslyn.reportFilePath")).thenReturn(new File("src/test/resources/CSharpSensorTest/roslyn-report.json").getAbsolutePath());
    sensor.analyse(project, context);

    assertThat(
      Files.toString(new File("src/test/resources/CSharpSensorTest/analysis-input.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", "")
        .replaceAll("<File>.*?Foo&amp;Bar.cs</File>", "<File>Foo&amp;Bar.cs</File>"))
      .isEqualTo(Files.toString(new File("src/test/resources/CSharpSensorTest/analysis-input-expected-with-roslyn.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", ""));
  }

}
