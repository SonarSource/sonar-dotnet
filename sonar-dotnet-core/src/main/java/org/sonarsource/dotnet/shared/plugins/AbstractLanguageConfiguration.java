/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import java.util.HashSet;
import java.util.Set;
import org.sonar.api.config.Configuration;
import org.sonar.api.scanner.ScannerSide;

import static java.util.Arrays.asList;

@ScannerSide
public abstract class AbstractLanguageConfiguration {
  protected final PluginMetadata metadata;

  protected final Configuration configuration;

  protected AbstractLanguageConfiguration(Configuration configuration, PluginMetadata metadata) {
    this.configuration = configuration;
    this.metadata = metadata;
  }

  public boolean ignoreThirdPartyIssues() {
    return configuration.getBoolean(AbstractPropertyDefinitions.getIgnoreIssuesProperty(metadata.languageKey())).orElse(false);
  }

  public Set<String> bugCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.getBugCategoriesProperty(metadata.languageKey()))));
  }

  public Set<String> codeSmellCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.getCodeSmellCategoriesProperty(metadata.languageKey()))));
  }

  public Set<String> vulnerabilityCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.getVulnerabilityCategoriesProperty(metadata.languageKey()))));
  }

  public boolean analyzeGeneratedCode() {
    return configuration.getBoolean(AbstractPropertyDefinitions.getAnalyzeGeneratedCode(metadata.languageKey())).orElse(false);
  }
}
