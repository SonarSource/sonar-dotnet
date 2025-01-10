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
package org.sonarsource.dotnet.shared.plugins;

import java.io.IOException;
import java.io.Writer;
import java.util.HashSet;
import java.util.Set;
import java.util.stream.Collectors;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;

public abstract class AbstractSonarLintProfileExporter extends ProfileExporter {
  private final String analyzerName;
  private final String repositoryKey;
  private final RuleFinder ruleFinder;

  public AbstractSonarLintProfileExporter(String profileKey, String profileName, String languageKey, String analyzerName, String repositoryKey, RuleFinder ruleFinder) {
    super(profileKey, profileName);
    this.ruleFinder = ruleFinder;
    setSupportedLanguages(languageKey);
    this.analyzerName = analyzerName;
    this.repositoryKey = repositoryKey;
  }

  @Override
  public void exportProfile(RulesProfile ruleProfile, Writer writer) {
    Set<String> allRuleKeys = ruleFinder.findAll(RuleQuery.create().withRepositoryKey(repositoryKey))
      .stream()
      .map(Rule::getKey)
      .collect(Collectors.toSet());
    Set<String> disabledRuleKeys = new HashSet<>(allRuleKeys);

    appendLine(writer, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    appendLine(writer, "<RuleSet Name=\"Rules for SonarLint\" Description=\"This rule set was automatically generated from SonarQube.\" ToolsVersion=\"14.0\">");
    appendLine(writer, "  <Rules AnalyzerId=\"" + analyzerName + "\" RuleNamespace=\"" + analyzerName + "\">");

    for (ActiveRule activeRule : ruleProfile.getActiveRulesByRepository(repositoryKey)) {
      Rule rule = activeRule.getRule();
      disabledRuleKeys.remove(rule.getKey());

      appendLine(writer, "    <Rule Id=\"" + rule.getKey() + "\" Action=\"Warning\" />");
    }

    for (String disableRuleKey : disabledRuleKeys) {
      appendLine(writer, "    <Rule Id=\"" + disableRuleKey + "\" Action=\"None\" />");
    }

    appendLine(writer, "  </Rules>");
    appendLine(writer, "</RuleSet>");
  }

  private static void appendLine(Writer writer, String line) {
    try {
      writer.write(line);
      writer.write("\r\n");
    } catch (IOException e) {
      throw new IllegalStateException(e);
    }
  }

}
