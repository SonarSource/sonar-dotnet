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

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.HashSet;
import java.util.Optional;
import java.util.Set;
import org.sonar.api.config.Configuration;
import org.sonar.api.scanner.ScannerSide;

import static java.util.Arrays.asList;

@ScannerSide
public abstract class AbstractLanguageConfiguration {
  private static final String SUFFIX = ".sonar";

  protected final PluginMetadata metadata;

  protected final Configuration configuration;

  protected AbstractLanguageConfiguration(Configuration configuration, PluginMetadata metadata) {
    this.configuration = configuration;
    this.metadata = metadata;
  }

  public boolean ignoreThirdPartyIssues() {
    return configuration.getBoolean(AbstractPropertyDefinitions.ignoreIssuesProperty(metadata.languageKey())).orElse(false);
  }

  public Set<String> bugCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.bugCategoriesProperty(metadata.languageKey()))));
  }

  public Set<String> codeSmellCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.codeSmellCategoriesProperty(metadata.languageKey()))));
  }

  public Set<String> vulnerabilityCategories() {
    return new HashSet<>(asList(configuration.getStringArray(AbstractPropertyDefinitions.vulnerabilityCategoriesProperty(metadata.languageKey()))));
  }

  public boolean analyzeGeneratedCode() {
    return configuration.getBoolean(AbstractPropertyDefinitions.analyzeGeneratedCode(metadata.languageKey())).orElse(false);
  }

  public Optional<Path> outputDir() {
    // Working directory folder is constructed from SonarOutputDir + ".sonar". We have to remove the suffix.
    // e.g. SonarOutputDir = .sonarqube\out\.sonar\ -> .sonarqube\out\
    return configuration
      .get("sonar.working.directory")
      .filter(s -> s.endsWith(SUFFIX))
      .map(AbstractLanguageConfiguration::outputDir);
  }

  private static Path outputDir(String workingDirectory) {
    return Paths.get(workingDirectory).getParent();
  }
}
