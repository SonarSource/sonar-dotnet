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

import java.util.ArrayList;
import java.util.List;
import org.sonar.api.PropertyType;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;
import org.sonar.api.utils.ManifestUtils;

/**
 * This class is responsible to declare all the properties that can be set through SonarQube UI (settings page).
 */
public abstract class AbstractPropertyDefinitions {
  private static final String EXTERNAL_ANALYZERS_CATEGORY = "External Analyzers";
  private static final String LEGACY_PROP_PREFIX = "sonaranalyzer-";  // properties that use this are legacy for compatibility with S4NET <= 9.0.2
  protected static final String PROP_PREFIX = "sonar.";
  public static final String PROJECT_KEY_PROPERTY = PROP_PREFIX + "projectKey";
  public static final String PROJECT_NAME_PROPERTY = PROP_PREFIX + "projectName";
  public static final String PROJECT_BASE_DIR_PROPERTY = PROP_PREFIX + "projectBaseDir";

  protected final PluginMetadata metadata;

  protected AbstractPropertyDefinitions(PluginMetadata metadata) {
    this.metadata = metadata;
  }

  public List<PropertyDefinition> create() {
    String languageKey = metadata.languageKey();
    String languageName = metadata.languageName();
    String version = projectVersion();
    List<PropertyDefinition> result = new ArrayList<>();
    result.add(
      PropertyDefinition.builder(roslynJsonReportPathProperty(languageKey))
        .multiValues(true)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(analyzerWorkDirProperty(languageKey))
        .multiValues(true)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(pluginKeyScannerPropertyKey(languageKey))
        .defaultValue(metadata.pluginKey())
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(pluginVersionScannerPropertyKey(languageKey))
        .defaultValue(version)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(staticResourceNameScannerPropertyKey(languageKey))
        .defaultValue("SonarAnalyzer-" + metadata.pluginKey() + "-" + version + ".zip")
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(fileSuffixProperty(languageKey))
        .category(languageName)
        .defaultValue(metadata.fileSuffixesDefaultValue())
        .name("File suffixes")
        .description("List of suffixes for files to analyze.")
        .multiValues(true)
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    result.add(
      PropertyDefinition.builder(ignoreHeaderCommentsProperty(languageKey))
        .category(languageName)
        .defaultValue("true")
        .name("Ignore header comments")
        .description("If set to \"true\", the file headers (that are usually the same on each file: " +
          "licensing information for example) are not considered as comments. Thus metrics such as \"Comment lines\" " +
          "do not get incremented. If set to \"false\", those file headers are considered as comments and metrics such as " +
          "\"Comment lines\" get incremented.")
        .onQualifiers(Qualifiers.PROJECT)
        .type(PropertyType.BOOLEAN)
        .build());
    result.add(
      PropertyDefinition.builder(analyzeGeneratedCode(languageKey))
        .category(languageName)
        .defaultValue("false")
        .name("Analyze generated code")
        .description("If set to \"true\", the files containing generated code are analyzed." +
          " If set to \"false\", the files containing generated code are ignored.")
        .onQualifiers(Qualifiers.PROJECT)
        .type(PropertyType.BOOLEAN)
        .build());
    result.add(
      PropertyDefinition.builder(ignoreIssuesProperty(languageKey))
        .type(PropertyType.BOOLEAN)
        .category(EXTERNAL_ANALYZERS_CATEGORY)
        .subCategory(languageName)
        .index(0)
        .defaultValue("false")
        .name("Ignore issues from external Roslyn analyzers")
        .description("If set to 'true', issues reported by external Roslyn analyzers won't be imported.")
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    result.add(
      PropertyDefinition.builder(bugCategoriesProperty(languageKey))
        .type(PropertyType.STRING)
        .multiValues(true)
        .category(EXTERNAL_ANALYZERS_CATEGORY)
        .subCategory(languageName)
        .index(1)
        .name("Rule categories associated with Bugs")
        .description("External rule categories to be treated as Bugs.")
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    result.add(
      PropertyDefinition.builder(vulnerabilityCategoriesProperty(languageKey))
        .type(PropertyType.STRING)
        .multiValues(true)
        .category(EXTERNAL_ANALYZERS_CATEGORY)
        .subCategory(languageName)
        .index(2)
        .name("Rule categories associated with Vulnerabilities")
        .description("External rule categories to be treated as Vulnerabilities.")
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    result.add(
      PropertyDefinition.builder(codeSmellCategoriesProperty(languageKey))
        .type(PropertyType.STRING)
        .multiValues(true)
        .category(EXTERNAL_ANALYZERS_CATEGORY)
        .subCategory(languageName)
        .index(3)
        .name("Rule categories associated with Code Smells")
        .description("External rule categories to be treated as Code Smells. By default, external issues are Code Smells, or Bugs when the severity is error.")
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    result.add(
      PropertyDefinition.builder(pluginKeyLegacyScannerPropertyKey(languageKey))
        .defaultValue(metadata.pluginKey())
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(pluginVersionLegacyScannerPropertyKey(languageKey))
        .defaultValue(version)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(staticResourceNameLegacyScannerPropertyKey(languageKey))
        .defaultValue("SonarAnalyzer-" + metadata.pluginKey() + "-" + version + ".zip")
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(analyzerIdLegacyScannerPropertyKey(languageKey))
        .defaultValue(metadata.analyzerProjectName())
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(ruleNamespaceLegacyScannerPropertyKey(languageKey))
        .defaultValue(metadata.analyzerProjectName())
        .hidden()
        .build());
    return result;
  }

  public static String ignoreHeaderCommentsProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".ignoreHeaderComments";
  }

  public static String analyzeGeneratedCode(String languageKey) {
    return PROP_PREFIX + languageKey + ".analyzeGeneratedCode";
  }

  public static String fileSuffixProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".file.suffixes";
  }

  public static String roslynJsonReportPathProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.reportFilePaths";
  }

  public static String analyzerWorkDirProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".analyzer.projectOutPaths";
  }

  public static String ignoreIssuesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.ignoreIssues";
  }

  public static String bugCategoriesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.bugCategories";
  }

  public static String codeSmellCategoriesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.codeSmellCategories";
  }

  public static String vulnerabilityCategoriesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.vulnerabilityCategories";
  }

  public static String pullRequestCacheBasePath() {
    return PROP_PREFIX + "pullrequest.cache.basepath";
  }

  public static String pullRequestBase() {
    return PROP_PREFIX + "pullrequest.base";
  }

  public static String pluginKeyScannerPropertyKey(String languageKey) {
    return scannerForDotNetProperty(languageKey, "pluginKey");
  }

  public static String pluginVersionScannerPropertyKey(String languageKey) {
    return scannerForDotNetProperty(languageKey, "pluginVersion");
  }

  public static String staticResourceNameScannerPropertyKey(String languageKey) {
    return scannerForDotNetProperty(languageKey, "staticResourceName");
  }

  private static String scannerForDotNetProperty(String languageKey, String name) {
    return PROP_PREFIX + languageKey + ".analyzer.dotnet." + name;
  }

  private static String pluginKeyLegacyScannerPropertyKey(String languageKey) {
    return LEGACY_PROP_PREFIX + languageKey + ".pluginKey";
  }

  private static String pluginVersionLegacyScannerPropertyKey(String languageKey) {
    return LEGACY_PROP_PREFIX + languageKey + ".pluginVersion";
  }

  private static String staticResourceNameLegacyScannerPropertyKey(String languageKey) {
    return LEGACY_PROP_PREFIX + languageKey + ".staticResourceName";
  }

  private static String analyzerIdLegacyScannerPropertyKey(String languageKey) {
    return LEGACY_PROP_PREFIX + languageKey + ".analyzerId";
  }

  private static String ruleNamespaceLegacyScannerPropertyKey(String languageKey) {
    return LEGACY_PROP_PREFIX + languageKey + ".ruleNamespace";
  }

  static String projectVersion() {
    List<String> propertyValues = ManifestUtils.getPropertyValues(AbstractPropertyDefinitions.class.getClassLoader(), "Plugin-Version");
    return propertyValues.isEmpty() ? "Version-N/A" : propertyValues.iterator().next();
  }
}
