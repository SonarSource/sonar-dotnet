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

import java.util.List;
import org.junit.Test;
import org.sonar.api.PropertyType;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.utils.ManifestUtils;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.mockStatic;
import static org.mockito.Mockito.when;

public class AbstractPropertyDefinitionsTest {

  @Test
  public void hidden_properties() {
    List<PropertyDefinition> properties = createProperties().stream()
      .filter(x -> !x.global() && x.configScopes().isEmpty()) // Hidden is non-global without scopes
      .toList();
    assertThat(properties)
      .extracting(PropertyDefinition::key, PropertyDefinition::defaultValue)
      .containsExactlyInAnyOrder(
        tuple("sonar.language-key.roslyn.reportFilePaths", ""),
        tuple("sonar.language-key.analyzer.projectOutPaths", ""),
        tuple("sonar.language-key.analyzer.dotnet.pluginKey", "plugin-key"),
        tuple("sonar.language-key.analyzer.dotnet.pluginVersion", "1.2.3.4"),
        tuple("sonar.language-key.analyzer.dotnet.staticResourceName", "SonarAnalyzer-plugin-key-1.2.3.4.zip"),
        tuple("sonar.language-key.analyzer.dotnet.analyzerId", "Project-Name"),
        tuple("sonar.language-key.analyzer.dotnet.ruleNamespace", "Project-Name"),
        tuple("sonaranalyzer-language-key.pluginKey", "plugin-key"),
        tuple("sonaranalyzer-language-key.pluginVersion", "1.2.3.4"),
        tuple("sonaranalyzer-language-key.staticResourceName", "SonarAnalyzer-plugin-key-1.2.3.4.zip"),
        tuple("sonaranalyzer-language-key.analyzerId", "Project-Name"),
        tuple("sonaranalyzer-language-key.ruleNamespace", "Project-Name"));
  }

  @Test
  public void scoped_properties() {
    List<PropertyDefinition> properties = createProperties().stream()
      .filter(x -> !x.global() && !x.configScopes().isEmpty())
      .toList();
    assertThat(properties).isEmpty();
  }

  @Test
  public void global_properties() {
    List<PropertyDefinition> properties = createProperties().stream()
      .filter(x -> x.global())
      .toList();
    assertThat(properties)
      .extracting(PropertyDefinition::type, PropertyDefinition::key, PropertyDefinition::category, PropertyDefinition::subCategory, PropertyDefinition::defaultValue)
      .containsExactlyInAnyOrder(
        tuple(PropertyType.STRING, "sonar.language-key.file.suffixes", "Language Name", "", null),
        tuple(PropertyType.BOOLEAN, "sonar.language-key.ignoreHeaderComments", "Language Name", "", "true"),
        tuple(PropertyType.BOOLEAN, "sonar.language-key.analyzeGeneratedCode", "Language Name", "", "false"),
        tuple(PropertyType.BOOLEAN, "sonar.language-key.roslyn.ignoreIssues", "External Analyzers", "Language Name", "false"),
        tuple(PropertyType.STRING, "sonar.language-key.roslyn.bugCategories", "External Analyzers", "Language Name", ""),
        tuple(PropertyType.STRING, "sonar.language-key.roslyn.vulnerabilityCategories", "External Analyzers", "Language Name", ""),
        tuple(PropertyType.STRING, "sonar.language-key.roslyn.codeSmellCategories", "External Analyzers", "Language Name", ""));
  }

  private List<PropertyDefinition> createProperties() {
    var sut = new AbstractPropertyDefinitions(metadata()) {
    };
    try (var manifestMock = mockStatic(ManifestUtils.class)) {
      manifestMock
        .when(() -> ManifestUtils.getPropertyValues(any(), eq("Plugin-Version")))
        .thenReturn(List.of("1.2.3.4"));

      return sut.create();
    }
  }

  private static PluginMetadata metadata() {
    PluginMetadata mock = mock(PluginMetadata.class);
    when(mock.pluginKey()).thenReturn("plugin-key");
    when(mock.languageKey()).thenReturn("language-key");
    when(mock.languageName()).thenReturn("Language Name");
    when(mock.analyzerProjectName()).thenReturn("Project-Name");
    return mock;
  }
}
