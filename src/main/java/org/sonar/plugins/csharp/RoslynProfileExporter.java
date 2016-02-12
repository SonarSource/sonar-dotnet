/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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
package org.sonar.plugins.csharp;

import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableMultimap;
import com.google.common.collect.Sets;
import java.io.File;
import java.io.IOException;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Set;
import org.apache.commons.codec.binary.Base64;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.config.Settings;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinition.Context;
import org.sonar.api.server.rule.RulesDefinition.Repository;
import org.sonar.api.server.rule.RulesDefinition.Rule;

public class RoslynProfileExporter extends ProfileExporter {

  private static final String SONARLINT_PARTIAL_REPO_KEY = "sonarlint-cs";
  private static final String SONARLINT_NUGET_VERSION = "1.7.0";

  private static final String ROSLYN_REPOSITORY_PREFIX = "roslyn.";

  private final Settings settings;
  private final RulesDefinition[] rulesDefinitions;

  public RoslynProfileExporter(Settings settings, RulesDefinition[] rulesDefinitions) {
    super("roslyn-cs", "Technical exporter for the MSBuild SonarQube Scanner");
    this.settings = settings;
    this.rulesDefinitions = rulesDefinitions;
    setSupportedLanguages(CSharpPlugin.LANGUAGE_KEY);
  }

  public static List<PropertyDefinition> sonarLintRepositoryProperties() {
    return Arrays.asList(
      PropertyDefinition.builder(pluginKeyPropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue("csharp")
        .hidden()
        .build(),
      PropertyDefinition.builder(pluginVersionPropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue(SONARLINT_NUGET_VERSION)
        .hidden()
        .build(),
      PropertyDefinition.builder(staticResourceNamePropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue("SonarLint.zip")
        .hidden()
        .build(),
      PropertyDefinition.builder(analyzerIdPropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue("SonarLint.CSharp")
        .hidden()
        .build(),
      PropertyDefinition.builder(ruleNamespacePropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue("SonarLint.CSharp")
        .hidden()
        .build(),
      PropertyDefinition.builder(nugetPackageIdPropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue("SonarLint")
        .hidden()
        .build(),
      PropertyDefinition.builder(nugetPackageVersionPropertyKey(SONARLINT_PARTIAL_REPO_KEY))
        .defaultValue(SONARLINT_NUGET_VERSION)
        .hidden()
        .build());
  }

  @Override
  public void exportProfile(RulesProfile rulesProfile, Writer writer) {
    ImmutableMultimap<String, ActiveRule> activeRoslynRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(rulesProfile.getActiveRules());

    appendLine(writer, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    appendLine(writer, "<RoslynExportProfile Version=\"1.0\">");

    appendLine(writer, "  <Configuration>");
    appendLine(writer, "    <RuleSet Name=\"Rules for SonarQube\" Description=\"This rule set was automatically generated from SonarQube.\" ToolsVersion=\"14.0\">");
    for (String partialRepoKey : activeRoslynRulesByPartialRepoKey.keySet()) {
      String analyzerId = mandatoryPropertyValue(analyzerIdPropertyKey(partialRepoKey));
      String ruleNamespace = mandatoryPropertyValue(ruleNamespacePropertyKey(partialRepoKey));

      appendLine(writer, "      <Rules AnalyzerId=\"" + escapeXml(analyzerId) + "\" RuleNamespace=\"" + escapeXml(ruleNamespace) + "\">");

      Set<String> activeRules = Sets.newHashSet();
      String repositoryKey = null;
      for (ActiveRule activeRule : activeRoslynRulesByPartialRepoKey.get(partialRepoKey)) {
        if (repositoryKey == null) {
          repositoryKey = activeRule.getRepositoryKey();
        }

        String ruleKey = activeRule.getRuleKey();
        activeRules.add(ruleKey);
        appendLine(writer, "        <Rule Id=\"" + escapeXml(ruleKey) + "\" Action=\"Warning\" />");
      }

      List<String> allRuleKeys = allRuleKeysByRepositoryKey(repositoryKey);
      for (String ruleKey : allRuleKeys) {
        if (!activeRules.contains(ruleKey)) {
          appendLine(writer, "        <Rule Id=\"" + escapeXml(ruleKey) + "\" Action=\"None\" />");
        }
      }

      appendLine(writer, "      </Rules>");
    }
    appendLine(writer, "    </RuleSet>");

    appendLine(writer, "    <AdditionalFiles>");

    String sonarlintParameters = CSharpSensor.analysisSettings(false, false, true, rulesProfile, Collections.<File>emptyList());
    String base64 = new String(Base64.encodeBase64(sonarlintParameters.getBytes(StandardCharsets.UTF_8)), StandardCharsets.UTF_8);
    appendLine(writer, "      <AdditionalFile FileName=\"SonarLint.xml\">" + base64 + "</AdditionalFile>");
    appendLine(writer, "    </AdditionalFiles>");

    appendLine(writer, "  </Configuration>");

    appendLine(writer, "  <Deployment>");

    appendLine(writer, "    <Plugins>");
    for (String partialRepoKey : activeRoslynRulesByPartialRepoKey.keySet()) {
      String pluginKey = mandatoryPropertyValue(pluginKeyPropertyKey(partialRepoKey));
      String pluginVersion = mandatoryPropertyValue(pluginVersionPropertyKey(partialRepoKey));
      String staticResourceName = mandatoryPropertyValue(staticResourceNamePropertyKey(partialRepoKey));

      appendLine(writer, "      <Plugin Key=\"" + escapeXml(pluginKey) + "\" Version=\"" + escapeXml(pluginVersion) + "\" StaticResourceName=\"" + escapeXml(staticResourceName) + "\" />");
    }
    appendLine(writer, "    </Plugins>");

    appendLine(writer, "    <NuGetPackages>");
    for (String partialRepoKey : activeRoslynRulesByPartialRepoKey.keySet()) {
      String packageId = mandatoryPropertyValue(nugetPackageIdPropertyKey(partialRepoKey));
      String packageVersion = mandatoryPropertyValue(nugetPackageVersionPropertyKey(partialRepoKey));

      appendLine(writer, "      <NuGetPackage Id=\"" + escapeXml(packageId) + "\" Version=\"" + escapeXml(packageVersion) + "\" />");
    }
    appendLine(writer, "    </NuGetPackages>");

    appendLine(writer, "  </Deployment>");

    appendLine(writer, "</RoslynExportProfile>");
  }

  public static ImmutableMultimap<String, ActiveRule> activeRoslynRulesByPartialRepoKey(List<ActiveRule> activeRules) {
    ImmutableMultimap.Builder<String, ActiveRule> builder = ImmutableMultimap.builder();

    for (ActiveRule activeRule : activeRules) {
      if (activeRule.getRepositoryKey().startsWith(ROSLYN_REPOSITORY_PREFIX)) {
        String pluginKey = activeRule.getRepositoryKey().substring(ROSLYN_REPOSITORY_PREFIX.length());
        builder.put(pluginKey, activeRule);
      } else if ("csharpsquid".equals(activeRule.getRepositoryKey())) {
        builder.put(SONARLINT_PARTIAL_REPO_KEY, activeRule);
      }
    }

    return builder.build();
  }

  private List<String> allRuleKeysByRepositoryKey(String repositoryKey) {
    ImmutableList.Builder<String> builder = ImmutableList.builder();
    for (RulesDefinition rulesDefinition : rulesDefinitions) {
      Context context = new Context();
      rulesDefinition.define(context);
      Repository repo = context.repository(repositoryKey);
      if (repo != null) {
        for (Rule rule : repo.rules()) {
          builder.add(rule.key());
        }
      }
    }
    return builder.build();
  }

  private String mandatoryPropertyValue(String propertyKey) {
    return Preconditions.checkNotNull(settings.getDefaultValue(propertyKey), "The mandatory property \"" + propertyKey + "\" must be set by the Roslyn plugin.");
  }

  private static String pluginKeyPropertyKey(String partialRepoKey) {
    return partialRepoKey + ".pluginKey";
  }

  private static String pluginVersionPropertyKey(String partialRepoKey) {
    return partialRepoKey + ".pluginVersion";
  }

  private static String staticResourceNamePropertyKey(String partialRepoKey) {
    return partialRepoKey + ".staticResourceName";
  }

  private static String analyzerIdPropertyKey(String partialRepoKey) {
    return partialRepoKey + ".analyzerId";
  }

  private static String ruleNamespacePropertyKey(String partialRepoKey) {
    return partialRepoKey + ".ruleNamespace";
  }

  private static String nugetPackageIdPropertyKey(String partialRepoKey) {
    return partialRepoKey + ".nuget.packageId";
  }

  private static String nugetPackageVersionPropertyKey(String partialRepoKey) {
    return partialRepoKey + ".nuget.packageVersion";
  }

  private static String escapeXml(String str) {
    return str.replace("&", "&amp;").replace("\"", "&quot;").replace("'", "&apos;").replace("<", "&lt;").replace(">", "&gt;");
  }

  private static void appendLine(Writer writer, String line) {
    try {
      writer.write(line);
      writer.write("\r\n");
    } catch (IOException e) {
      Throwables.propagate(e);
    }
  }

}
