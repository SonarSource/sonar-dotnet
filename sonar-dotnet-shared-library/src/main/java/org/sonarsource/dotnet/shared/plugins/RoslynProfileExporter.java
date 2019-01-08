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

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.RuleParam;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinition.Context;
import org.sonar.api.server.rule.RulesDefinition.Repository;
import org.sonar.api.server.rule.RulesDefinition.Rule;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import static java.util.stream.Collectors.toList;

public class RoslynProfileExporter extends ProfileExporter {
  private static final Logger LOG = Loggers.get(RoslynProfileExporter.class);
  private static final String ROSLYN_REPOSITORY_PREFIX = "roslyn.";

  private final DotNetPluginMetadata pluginMetadata;
  private final Configuration configuration;
  private final RulesDefinition[] rulesDefinitions;

  public RoslynProfileExporter(DotNetPluginMetadata pluginMetadata, Configuration configuration, RulesDefinition[] rulesDefinitions) {
    super(profileKey(pluginMetadata), "Technical exporter for the MSBuild SonarQube Scanner");
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
    this.rulesDefinitions = rulesDefinitions;
    setSupportedLanguages(pluginMetadata.languageKey());
  }

  private static String sonarAnalyzerPartialRepoKey(DotNetPluginMetadata pluginMetadata) {
    return "sonaranalyzer-" + pluginMetadata.languageKey();
  }

  private static String profileKey(DotNetPluginMetadata pluginMetadata) {
    return "roslyn-" + pluginMetadata.languageKey();
  }

  public static List<PropertyDefinition> sonarLintRepositoryProperties(DotNetPluginMetadata pluginMetadata) {
    String analyzerVersion = getAnalyzerVersion();
    return Arrays.asList(
      PropertyDefinition.builder(pluginKeyPropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue(pluginMetadata.pluginKey())
        .hidden()
        .build(),
      PropertyDefinition.builder(pluginVersionPropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue(analyzerVersion)
        .hidden()
        .build(),
      PropertyDefinition.builder(staticResourceNamePropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue("SonarAnalyzer-" + analyzerVersion + ".zip")
        .hidden()
        .build(),
      PropertyDefinition.builder(analyzerIdPropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue(pluginMetadata.sonarAnalyzerName())
        .hidden()
        .build(),
      PropertyDefinition.builder(ruleNamespacePropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue(pluginMetadata.sonarAnalyzerName())
        .hidden()
        .build(),
      PropertyDefinition.builder(nugetPackageIdPropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue(pluginMetadata.sonarAnalyzerName())
        .hidden()
        .build(),
      PropertyDefinition.builder(nugetPackageVersionPropertyKey(sonarAnalyzerPartialRepoKey(pluginMetadata)))
        .defaultValue(analyzerVersion)
        .hidden()
        .build());
  }

  private static String getAnalyzerVersion() {
    try {
      return new BufferedReader(new InputStreamReader(RoslynProfileExporter.class.getResourceAsStream("/static/version.txt"), StandardCharsets.UTF_8)).readLine();
    } catch (IOException e) {
      throw new IllegalStateException("Couldn't read C# analyzer version number from '/static/version.txt'", e);
    }
  }

  @Override
  public void exportProfile(RulesProfile rulesProfile, Writer writer) {
    try {
      Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(pluginMetadata, rulesProfile.getActiveRules()
        .stream()
        .map(r -> RuleKey.of(r.getRepositoryKey(), r.getRuleKey()))
        .collect(toList()));

      appendLine(writer, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
      appendLine(writer, "<RoslynExportProfile Version=\"1.0\">");

      appendLine(writer, "  <Configuration>");
      appendLine(writer, "    <RuleSet Name=\"Rules for SonarQube\" Description=\"This rule set was automatically generated from SonarQube.\" ToolsVersion=\"14.0\">");
      for (Map.Entry<String, List<RuleKey>> partialRepoEntry : activeRoslynRulesByPartialRepoKey.entrySet()) {
        writeRepoRuleSet(partialRepoEntry.getKey(), partialRepoEntry.getValue(), writer);
      }
      appendLine(writer, "    </RuleSet>");

      appendLine(writer, "    <AdditionalFiles>");

      String sonarlintParameters = analysisSettings(false, false, true, rulesProfile);
      java.util.Base64.Encoder encoder = java.util.Base64.getEncoder();
      String base64 = new String(encoder.encode(sonarlintParameters.getBytes(StandardCharsets.UTF_8)), StandardCharsets.UTF_8);
      appendLine(writer, "      <AdditionalFile FileName=\"SonarLint.xml\">" + base64 + "</AdditionalFile>");
      appendLine(writer, "    </AdditionalFiles>");

      appendLine(writer, "  </Configuration>");

      appendLine(writer, "  <Deployment>");

      appendLine(writer, "    <Plugins>");
      for (String partialRepoKey : activeRoslynRulesByPartialRepoKey.keySet()) {
        String pluginKey = mandatoryPropertyValue(pluginKeyPropertyKey(partialRepoKey));
        String pluginVersion = mandatoryPropertyValue(pluginVersionPropertyKey(partialRepoKey));
        String staticResourceName = mandatoryPropertyValue(staticResourceNamePropertyKey(partialRepoKey));

        appendLine(writer,
          "      <Plugin Key=\"" + escapeXml(pluginKey) + "\" Version=\"" + escapeXml(pluginVersion) + "\" StaticResourceName=\"" + escapeXml(staticResourceName) + "\" />");
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
    } catch (Exception e) {
      LOG.error(String.format("Error exporting profile '%s' for language '%s'", rulesProfile.getName(), rulesProfile.getLanguage()), e);
      throw e;
    }
  }

  private void writeRepoRuleSet(String partialRepoKey, Collection<RuleKey> ruleKeys, Writer writer) {
    String analyzerId = mandatoryPropertyValue(analyzerIdPropertyKey(partialRepoKey));
    String ruleNamespace = mandatoryPropertyValue(ruleNamespacePropertyKey(partialRepoKey));

    appendLine(writer, "      <Rules AnalyzerId=\"" + escapeXml(analyzerId) + "\" RuleNamespace=\"" + escapeXml(ruleNamespace) + "\">");

    Set<String> activeRules = new LinkedHashSet<>();
    String repositoryKey = null;
    for (RuleKey activeRuleKey : ruleKeys) {
      if (repositoryKey == null) {
        repositoryKey = activeRuleKey.repository();
      }

      String ruleKey = activeRuleKey.rule();
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

  private String analysisSettings(boolean includeSettings, boolean ignoreHeaderComments, boolean includeRules, RulesProfile ruleProfile) {
    StringBuilder sb = new StringBuilder();

    appendLine(sb, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    appendLine(sb, "<AnalysisInput>");

    if (includeSettings) {
      appendLine(sb, "  <Settings>");
      appendLine(sb, "    <Setting>");
      appendLine(sb, "      <Key>" + pluginMetadata.ignoreHeaderCommentPropertyKey() + "</Key>");
      appendLine(sb, "      <Value>" + (ignoreHeaderComments ? "true" : "false") + "</Value>");
      appendLine(sb, "    </Setting>");
      appendLine(sb, "  </Settings>");
    }

    appendLine(sb, "  <Rules>");
    if (includeRules) {
      for (ActiveRule activeRule : ruleProfile.getActiveRulesByRepository(pluginMetadata.repositoryKey())) {
        appendLine(sb, "    <Rule>");
        appendLine(sb, "      <Key>" + escapeXml(activeRule.getRuleKey()) + "</Key>");
        appendParameters(sb, effectiveParameters(activeRule));
        appendLine(sb, "    </Rule>");
      }
    }
    appendLine(sb, "  </Rules>");

    appendLine(sb, "  <Files>");
    appendLine(sb, "  </Files>");

    appendLine(sb, "</AnalysisInput>");

    return sb.toString();
  }

  private static void appendParameters(StringBuilder sb, Map<String, String> parameters) {
    if (!parameters.isEmpty()) {
      appendLine(sb, "      <Parameters>");
      for (Map.Entry<String, String> parameter : parameters.entrySet()) {
        appendLine(sb, "        <Parameter>");
        appendLine(sb, "          <Key>" + escapeXml(parameter.getKey()) + "</Key>");
        appendLine(sb, "          <Value>" + escapeXml(parameter.getValue()) + "</Value>");
        appendLine(sb, "        </Parameter>");
      }
      appendLine(sb, "      </Parameters>");
    }
  }

  private static Map<String, String> effectiveParameters(ActiveRule activeRule) {
    Map<String, String> result = new HashMap<>();

    if (activeRule.getRule().getTemplate() != null) {
      result.put("RuleKey", activeRule.getRuleKey());
    }

    for (ActiveRuleParam param : activeRule.getActiveRuleParams()) {
      result.put(param.getKey(), param.getValue() == null ? "" : param.getValue());
    }

    for (RuleParam param : activeRule.getRule().getParams()) {
      if (!result.containsKey(param.getKey())) {
        result.put(param.getKey(), param.getDefaultValue() == null ? "" : param.getDefaultValue());
      }
    }

    return result;
  }

  public static Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey(DotNetPluginMetadata pluginMetadata, Iterable<RuleKey> activeRules) {
    Map<String, List<RuleKey>> result = new LinkedHashMap<>();

    for (RuleKey activeRule : activeRules) {
      if (activeRule.repository().startsWith(ROSLYN_REPOSITORY_PREFIX)) {
        String pluginKey = activeRule.repository().substring(ROSLYN_REPOSITORY_PREFIX.length());
        result.putIfAbsent(pluginKey, new ArrayList<>());
        result.get(pluginKey).add(activeRule);
      } else if (pluginMetadata.repositoryKey().equals(activeRule.repository())) {
        result.putIfAbsent(sonarAnalyzerPartialRepoKey(pluginMetadata), new ArrayList<>());
        result.get(sonarAnalyzerPartialRepoKey(pluginMetadata)).add(activeRule);
      }
    }

    return result;
  }

  private List<String> allRuleKeysByRepositoryKey(String repositoryKey) {
    List<String> result = new ArrayList<>();
    for (RulesDefinition rulesDefinition : rulesDefinitions) {
      Context context = new Context();
      rulesDefinition.define(context);
      Repository repo = context.repository(repositoryKey);
      if (repo != null) {
        for (Rule rule : repo.rules()) {
          result.add(rule.key());
        }
      }
    }
    return result;
  }

  private String mandatoryPropertyValue(String propertyKey) {
    return configuration.get(propertyKey).orElseThrow(() -> new IllegalStateException("The mandatory property \"" + propertyKey + "\" must be set by the Roslyn plugin."));
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
      throw new IllegalStateException(e);
    }
  }

  private static void appendLine(StringBuilder sb, String line) {
    sb.append(line);
    sb.append("\r\n");
  }
}
