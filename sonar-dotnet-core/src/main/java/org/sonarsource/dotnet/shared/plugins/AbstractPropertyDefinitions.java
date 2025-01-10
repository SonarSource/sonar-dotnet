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
      PropertyDefinition.builder(getRoslynJsonReportPathProperty(metadata.languageKey()))
        .multiValues(true)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(getAnalyzerWorkDirProperty(languageKey))
        .multiValues(true)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(pluginKeyPropertyKey(languageKey))
        .defaultValue(metadata.pluginKey())
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(pluginVersionPropertyKey(languageKey))
        .defaultValue(version)
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(staticResourceNamePropertyKey(languageKey))
        .defaultValue("SonarAnalyzer-" + metadata.pluginKey() + "-" + version + ".zip")
        .hidden()
        .build());
    result.add(
      PropertyDefinition.builder(getFileSuffixProperty(languageKey))
        .category(languageName)
        .defaultValue(metadata.fileSuffixesDefaultValue())
        .name("File suffixes")
        .description("List of suffixes for files to analyze.")
        .multiValues(true)
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    result.add(
      PropertyDefinition.builder(getIgnoreHeaderCommentsProperty(languageKey))
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
      PropertyDefinition.builder(getAnalyzeGeneratedCode(languageKey))
        .category(languageName)
        .defaultValue("false")
        .name("Analyze generated code")
        .description("If set to \"true\", the files containing generated code are analyzed." +
          " If set to \"false\", the files containing generated code are ignored.")
        .onQualifiers(Qualifiers.PROJECT)
        .type(PropertyType.BOOLEAN)
        .build());
    result.add(
      PropertyDefinition.builder(getIgnoreIssuesProperty(languageKey))
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
      PropertyDefinition.builder(getBugCategoriesProperty(languageKey))
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
      PropertyDefinition.builder(getVulnerabilityCategoriesProperty(languageKey))
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
      PropertyDefinition.builder(getCodeSmellCategoriesProperty(languageKey))
        .type(PropertyType.STRING)
        .multiValues(true)
        .category(EXTERNAL_ANALYZERS_CATEGORY)
        .subCategory(languageName)
        .index(3)
        .name("Rule categories associated with Code Smells")
        .description("External rule categories to be treated as Code Smells. By default, external issues are Code Smells, or Bugs when the severity is error.")
        .onQualifiers(Qualifiers.PROJECT)
        .build());
    return result;
  }

  public static String getIgnoreHeaderCommentsProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".ignoreHeaderComments";
  }

  public static String getAnalyzeGeneratedCode(String languageKey) {
    return PROP_PREFIX + languageKey + ".analyzeGeneratedCode";
  }

  public static String getFileSuffixProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".file.suffixes";
  }

  public static String getRoslynJsonReportPathProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.reportFilePaths";
  }

  public static String getAnalyzerWorkDirProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".analyzer.projectOutPaths";
  }

  public static String getIgnoreIssuesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.ignoreIssues";
  }

  public static String getBugCategoriesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.bugCategories";
  }

  public static String getCodeSmellCategoriesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.codeSmellCategories";
  }

  public static String getVulnerabilityCategoriesProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".roslyn.vulnerabilityCategories";
  }

  public static String getPullRequestCacheBasePath() {
    return PROP_PREFIX + "pullrequest.cache.basepath";
  }

  public static String getPullRequestBase() {
    return PROP_PREFIX + "pullrequest.base";
  }

  public static String pluginKeyPropertyKey(String languageKey) {
    return scannerForDotNetProperty(languageKey, "pluginKey");
  }

  public static String pluginVersionPropertyKey(String languageKey) {
    return scannerForDotNetProperty(languageKey, "pluginVersion");
  }

  public static String staticResourceNamePropertyKey(String languageKey) {
    return scannerForDotNetProperty(languageKey, "staticResourceName");
  }

  private static String scannerForDotNetProperty(String languageKey, String name) {
    return PROP_PREFIX + languageKey + ".analyzer.dotnet." + name;
  }

  static String projectVersion() {
    List<String> propertyValues = ManifestUtils.getPropertyValues(AbstractPropertyDefinitions.class.getClassLoader(), "Plugin-Version");
    return propertyValues.isEmpty() ? "Version-N/A" : propertyValues.iterator().next();
  }
}
