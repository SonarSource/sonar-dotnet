/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import java.util.HashSet;
import java.util.Set;
import org.sonar.api.config.Configuration;
import org.sonar.api.scanner.ScannerSide;

import static java.util.Arrays.asList;

@ScannerSide
public abstract class AbstractLanguageConfiguration {
  private final String languageKey;

  protected final Configuration configuration;

  protected AbstractLanguageConfiguration(Configuration configuration, PluginMetadata metadata) {
    this.configuration = configuration;
    this.languageKey = metadata.languageKey();
  }

  public boolean ignoreThirdPartyIssues() {
    return configuration.getBoolean(AbstractPropertyDefinitions.getIgnoreIssuesProperty(languageKey)).orElse(false);
  }

  public Set<String> bugCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.getBugCategoriesProperty(languageKey))));
  }

  public Set<String> codeSmellCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.getCodeSmellCategoriesProperty(languageKey))));
  }

  public Set<String> vulnerabilityCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.getVulnerabilityCategoriesProperty(languageKey))));
  }

  public boolean analyzeGeneratedCode() {
    return configuration.getBoolean(AbstractPropertyDefinitions.getAnalyzeGeneratedCode(languageKey)).orElse(false);
  }
}
