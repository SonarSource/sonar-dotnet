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
package org.sonarsource.dotnet.shared.plugins;

import com.google.common.io.Files;
import java.io.File;
import java.io.IOException;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.config.Configuration;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rule.Severity;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.fail;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.sonarsource.dotnet.shared.plugins.RoslynProfileExporter.activeRoslynRulesByPartialRepoKey;

public class RoslynProfileExporterTest {

  public static final String SONAR_ANALYZER_NAME = "SonarAnalyzer.CSharp";

  @org.junit.Rule
  public ExpectedException thrown = ExpectedException.none();

  @org.junit.Rule
  public LogTester logs = new LogTester();

  private DotNetPluginMetadata pluginMetadata = csPluginMetadata();

  private static DotNetPluginMetadata csPluginMetadata() {
    DotNetPluginMetadata metadata = mock(DotNetPluginMetadata.class);
    when(metadata.languageKey()).thenReturn("cs");
    when(metadata.repositoryKey()).thenReturn("csharpsquid");
    when(metadata.sonarAnalyzerName()).thenReturn(SONAR_ANALYZER_NAME);
    return metadata;
  }

  @Test
  public void no_rules() throws Exception {
    RoslynProfileExporter exporter = new RoslynProfileExporter(pluginMetadata, mock(Configuration.class), new RulesDefinition[0]);
    assertThat(exporter.getKey()).isEqualTo("roslyn-cs");
    assertThat(exporter.getName()).isEqualTo("Technical exporter for the MSBuild SonarQube Scanner");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");

    StringWriter writer = new StringWriter();
    exporter.exportProfile(mock(RulesProfile.class), writer);

    String actual = writer.toString().replaceAll("\r?\n|\r", "");
    String expected = Files.toString(new File("src/test/resources/RoslynProfileExporterTest/no_rules.xml"), StandardCharsets.UTF_8).replaceAll("\r?\n|\r", "");
    assertThat(actual).isEqualTo(expected);
  }

  @Test
  public void only_sonarlint() throws Exception {
    ActiveRule templateActiveRule = mock(ActiveRule.class);
    when(templateActiveRule.getRepositoryKey()).thenReturn("csharpsquid");
    when(templateActiveRule.getRuleKey()).thenReturn("[template_key\"'<>&]");
    Rule templateRule = mock(Rule.class);
    Rule baseTemplateRule = mock(Rule.class);
    when(baseTemplateRule.getKey()).thenReturn("[base_key]");
    when(templateRule.getTemplate()).thenReturn(baseTemplateRule);
    when(templateActiveRule.getRule()).thenReturn(templateRule);

    ActiveRule parametersActiveRule = mock(ActiveRule.class);
    when(parametersActiveRule.getRepositoryKey()).thenReturn("csharpsquid");
    when(parametersActiveRule.getRuleKey()).thenReturn("[parameters_key]");
    ActiveRuleParam param1 = mock(ActiveRuleParam.class);
    when(param1.getKey()).thenReturn("[param1_key]");
    when(param1.getValue()).thenReturn("[param1_value]");
    when(parametersActiveRule.getActiveRuleParams()).thenReturn(Collections.singletonList(param1));
    Rule parametersRule = mock(Rule.class);
    RuleParam param1Default = mock(org.sonar.api.rules.RuleParam.class);
    when(param1Default.getKey()).thenReturn("[param1_key]");
    when(param1Default.getDefaultValue()).thenReturn("[param1_default_value]");
    RuleParam param2Default = mock(org.sonar.api.rules.RuleParam.class);
    when(param2Default.getKey()).thenReturn("[param2_default_key]");
    when(param2Default.getDefaultValue()).thenReturn("[param2_default_value]");
    when(parametersRule.getParams()).thenReturn(Arrays.asList(param1Default, param2Default));
    when(parametersActiveRule.getRule()).thenReturn(parametersRule);

    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository("csharpsquid")).thenReturn(Arrays.asList(templateActiveRule, parametersActiveRule));
    when(rulesProfile.getActiveRules()).thenReturn(Arrays.asList(templateActiveRule, parametersActiveRule));

    Configuration configuration = mock(Configuration.class);
    when(configuration.get("sonaranalyzer-cs.pluginKey")).thenReturn(Optional.of("csharp"));
    when(configuration.get("sonaranalyzer-cs.pluginVersion")).thenReturn(Optional.of("1.7.0"));
    when(configuration.get("sonaranalyzer-cs.staticResourceName")).thenReturn(Optional.of("SonarAnalyzer.zip"));
    when(configuration.get("sonaranalyzer-cs.analyzerId")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.ruleNamespace")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.nuget.packageId")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.nuget.packageVersion")).thenReturn(Optional.of("1.10.0"));

    RulesDefinition sonarLintRepo = context -> {
      RulesDefinition.NewRepository repo = context.createRepository("csharpsquid", "cs");
      repo.createRule("InactiveRule").setName("InactiveRule").setMarkdownDescription("InactiveRule").setSeverity(Severity.MAJOR);
      repo.done();
    };

    RoslynProfileExporter exporter = new RoslynProfileExporter(pluginMetadata, configuration, new RulesDefinition[] {sonarLintRepo});
    assertThat(exporter.getKey()).isEqualTo("roslyn-cs");
    assertThat(exporter.getName()).isEqualTo("Technical exporter for the MSBuild SonarQube Scanner");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");

    StringWriter writer = new StringWriter();
    exporter.exportProfile(rulesProfile, writer);

    String actual = writer.toString().replaceAll("\r?\n|\r", "");
    String expected = Files.toString(new File("src/test/resources/RoslynProfileExporterTest/only_sonarlint.xml"), StandardCharsets.UTF_8).replaceAll("\r?\n|\r", "");
    assertThat(actual).isEqualTo(expected);
  }

  @Test
  public void mixed_sonarlint_and_custom_rules() throws Exception {
    Rule sonarLintRule = mock(Rule.class);
    ActiveRule sonarLintActiveRule = mock(ActiveRule.class);
    when(sonarLintActiveRule.getRepositoryKey()).thenReturn("csharpsquid");
    when(sonarLintActiveRule.getRuleKey()).thenReturn("S1000");
    when(sonarLintActiveRule.getRule()).thenReturn(sonarLintRule);

    Rule customRoslynRule = mock(Rule.class);
    ActiveRule customRoslynActiveRule = mock(ActiveRule.class);
    when(customRoslynActiveRule.getRepositoryKey()).thenReturn("roslyn.custom");
    when(customRoslynActiveRule.getRuleKey()).thenReturn("CA1000");
    when(customRoslynActiveRule.getRule()).thenReturn(customRoslynRule);

    Rule fxcopRule = mock(Rule.class);
    ActiveRule fxcopActiveRule = mock(ActiveRule.class);
    when(fxcopActiveRule.getRepositoryKey()).thenReturn("fxcop");
    when(fxcopActiveRule.getRuleKey()).thenReturn("CA2000");
    when(fxcopActiveRule.getRule()).thenReturn(fxcopRule);

    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository("csharpsquid")).thenReturn(Collections.singletonList(sonarLintActiveRule));

    when(rulesProfile.getActiveRules()).thenReturn(Arrays.asList(sonarLintActiveRule, customRoslynActiveRule, fxcopActiveRule));

    Configuration configuration = mock(Configuration.class);
    when(configuration.get("sonaranalyzer-cs.pluginKey")).thenReturn(Optional.of("csharp"));
    when(configuration.get("sonaranalyzer-cs.pluginVersion")).thenReturn(Optional.of("1.7.0"));
    when(configuration.get("sonaranalyzer-cs.staticResourceName")).thenReturn(Optional.of("SonarAnalyzer.zip"));
    when(configuration.get("sonaranalyzer-cs.analyzerId")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.ruleNamespace")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.nuget.packageId")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.nuget.packageVersion")).thenReturn(Optional.of("1.10.0"));

    when(configuration.get("custom.pluginKey")).thenReturn(Optional.of("customPluginKey"));
    when(configuration.get("custom.pluginVersion")).thenReturn(Optional.of("customPluginVersion"));
    when(configuration.get("custom.staticResourceName")).thenReturn(Optional.of("customPluginStaticResourceName"));
    when(configuration.get("custom.analyzerId")).thenReturn(Optional.of("custom-roslyn"));
    when(configuration.get("custom.ruleNamespace")).thenReturn(Optional.of("custom-roslyn-namespace"));
    when(configuration.get("custom.nuget.packageId")).thenReturn(Optional.of("custom-roslyn-package"));
    when(configuration.get("custom.nuget.packageVersion")).thenReturn(Optional.of("custom-rolsyn-version"));

    RulesDefinition sonarLintRepo = context -> {
      RulesDefinition.NewRepository repo = context.createRepository("csharpsquid", "cs");
      repo.createRule("S1000").setName("S1000").setMarkdownDescription("S1000").setSeverity(Severity.MAJOR);
      repo.createRule("SonarLintInactiveRule").setName("InactiveRule").setMarkdownDescription("InactiveRule").setSeverity(Severity.MAJOR);
      repo.done();
    };

    RulesDefinition customRoslynRepo = context -> {
      RulesDefinition.NewRepository repo = context.createRepository("roslyn.custom", "cs");
      repo.createRule("CA1000").setName("CA1000").setMarkdownDescription("CA1000").setSeverity(Severity.MAJOR);
      repo.createRule("CustomRoslynInactiverule1").setName("InactiveRule").setMarkdownDescription("InactiveRule").setSeverity(Severity.MAJOR);
      repo.done();
    };

    RulesDefinition fxcopRepo = context -> {
      RulesDefinition.NewRepository repo = context.createRepository("fxcop", "cs");
      repo.createRule("CA2000").setName("CA1000").setMarkdownDescription("CA1000").setSeverity(Severity.MAJOR);
      repo.done();
    };

    RulesDefinition stylecopRepo = context -> {
      RulesDefinition.NewRepository repo = context.createRepository("stylecop", "cs");
      repo.createRule("SC1000").setName("SC1000").setMarkdownDescription("SC1000").setSeverity(Severity.MAJOR);
      repo.done();
    };

    RulesDefinition[] rulesDefinitions = {sonarLintRepo, customRoslynRepo, fxcopRepo, stylecopRepo};
    RoslynProfileExporter exporter = new RoslynProfileExporter(pluginMetadata, configuration, rulesDefinitions);
    assertThat(exporter.getKey()).isEqualTo("roslyn-cs");
    assertThat(exporter.getName()).isEqualTo("Technical exporter for the MSBuild SonarQube Scanner");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");

    StringWriter writer = new StringWriter();
    exporter.exportProfile(rulesProfile, writer);

    String actual = writer.toString().replaceAll("\r?\n|\r", "");
    String expected = Files.toString(new File("src/test/resources/RoslynProfileExporterTest/mixed.xml"), StandardCharsets.UTF_8).replaceAll("\r?\n|\r", "");
    assertThat(actual).isEqualTo(expected);
  }

  @Test
  public void should_fail_fast_with_incomplete_plugin_metadata() {
    Rule rule = mock(Rule.class);
    ActiveRule activeRule = mock(ActiveRule.class);
    when(activeRule.getRepositoryKey()).thenReturn("roslyn.foo");
    when(activeRule.getRuleKey()).thenReturn("CA1000");
    when(activeRule.getRule()).thenReturn(rule);

    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRules()).thenReturn(Collections.singletonList(activeRule));

    RoslynProfileExporter exporter = new RoslynProfileExporter(pluginMetadata, mock(Configuration.class), new RulesDefinition[0]);

    try {
      exporter.exportProfile(rulesProfile, new StringWriter());
      fail("was expecting an exception");
    } catch (IllegalStateException ex) {
      assertThat(ex.getMessage()).isEqualTo("The mandatory property \"foo.analyzerId\" must be set by the Roslyn plugin.");
      assertThat(logs.logs()).containsOnly("Error exporting profile 'null' for language 'null'");
    }
  }

  @Test
  public void ruleWithParameterWithNullValue() throws IOException {
    Configuration configuration = mock(Configuration.class);
    when(configuration.get("sonaranalyzer-cs.pluginKey")).thenReturn(Optional.of("csharp"));
    when(configuration.get("sonaranalyzer-cs.pluginVersion")).thenReturn(Optional.of("1.7.0"));
    when(configuration.get("sonaranalyzer-cs.staticResourceName")).thenReturn(Optional.of("SonarAnalyzer.zip"));
    when(configuration.get("sonaranalyzer-cs.analyzerId")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.ruleNamespace")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.nuget.packageId")).thenReturn(Optional.of(SONAR_ANALYZER_NAME));
    when(configuration.get("sonaranalyzer-cs.nuget.packageVersion")).thenReturn(Optional.of("1.10.0"));

    RulesDefinition sonarLintRepo = context -> {
      RulesDefinition.NewRepository repo = context.createRepository("csharpsquid", "cs");
      repo.createRule("S1000")
        .setName("S1000")
        .setMarkdownDescription("S1000")
        .setSeverity(Severity.MAJOR);
      repo.done();
    };

    Rule sonarLintRule = mock(Rule.class);
    ActiveRule sonarLintActiveRule = mock(ActiveRule.class);
    when(sonarLintActiveRule.getRepositoryKey()).thenReturn("csharpsquid");
    when(sonarLintActiveRule.getRuleKey()).thenReturn("S1000");
    when(sonarLintActiveRule.getRule()).thenReturn(sonarLintRule);
    ActiveRuleParam param = mock(ActiveRuleParam.class);
    when(param.getKey()).thenReturn("param");
    when(param.getValue()).thenReturn(null);
    when(sonarLintActiveRule.getActiveRuleParams()).thenReturn(Collections.singletonList(param));

    RulesProfile rulesProfile = mock(RulesProfile.class);
    when(rulesProfile.getActiveRulesByRepository("csharpsquid")).thenReturn(Collections.singletonList(sonarLintActiveRule));
    when(rulesProfile.getActiveRules()).thenReturn(Collections.singletonList(sonarLintActiveRule));
    when(rulesProfile.getLanguage()).thenReturn("csharp");
    when(rulesProfile.getName()).thenReturn("myprofile");

    RoslynProfileExporter exporter = new RoslynProfileExporter(pluginMetadata, configuration, new RulesDefinition[] {sonarLintRepo});
    StringWriter writer = new StringWriter();
    exporter.exportProfile(rulesProfile, writer);

    String actual = writer.toString().replaceAll("\r?\n|\r", "");
    String expected = Files.toString(new File("src/test/resources/RoslynProfileExporterTest/empty_string_value.xml"), StandardCharsets.UTF_8).replaceAll("\r?\n|\r", "");
    assertThat(actual).isEqualTo(expected);
  }

  @Test
  public void activeRoslynRulesByPluginKey() {
    assertThat(activeRoslynRulesByPartialRepoKey(pluginMetadata, Collections.emptyList()).size()).isEqualTo(0);

    RuleKey randomActiveRuleKey = mock(RuleKey.class);
    when(randomActiveRuleKey.rule()).thenReturn("1");
    when(randomActiveRuleKey.repository()).thenReturn("1");
    assertThat(activeRoslynRulesByPartialRepoKey(pluginMetadata, Collections.singletonList(randomActiveRuleKey)).size()).isEqualTo(0);

    RuleKey sonarLintActiveRuleKey = mock(RuleKey.class);
    when(sonarLintActiveRuleKey.rule()).thenReturn("2");
    when(sonarLintActiveRuleKey.repository()).thenReturn("csharpsquid");
    Map<String, List<RuleKey>> activeRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(pluginMetadata, Collections.singletonList(sonarLintActiveRuleKey));
    assertThat(activeRulesByPartialRepoKey.size()).isEqualTo(1);
    assertThat(activeRulesByPartialRepoKey.get("sonaranalyzer-cs")).containsOnly(sonarLintActiveRuleKey);

    RuleKey customRoslynActiveRuleKey = mock(RuleKey.class);
    when(customRoslynActiveRuleKey.rule()).thenReturn("3");
    when(customRoslynActiveRuleKey.repository()).thenReturn("roslyn.foo");
    activeRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(pluginMetadata, Collections.singletonList(customRoslynActiveRuleKey));
    assertThat(activeRulesByPartialRepoKey.size()).isEqualTo(1);
    assertThat(activeRulesByPartialRepoKey.get("foo")).containsOnly(customRoslynActiveRuleKey);

    activeRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(pluginMetadata,
      Arrays.asList(
        randomActiveRuleKey,
        sonarLintActiveRuleKey,
        customRoslynActiveRuleKey));
    assertThat(activeRulesByPartialRepoKey.size()).isEqualTo(2);
    assertThat(activeRulesByPartialRepoKey.get("sonaranalyzer-cs")).containsOnly(sonarLintActiveRuleKey);
    assertThat(activeRulesByPartialRepoKey.get("foo")).containsOnly(customRoslynActiveRuleKey);
  }

}
