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

import java.util.ArrayList;
import java.util.List;
import org.sonar.api.PropertyType;
import org.sonar.api.SonarRuntime;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;
import org.sonar.api.utils.Version;

/**
 * This class is responsible to declare all the properties that can be set through SonarQube/SonarCloud UI (settings page).
 */
public abstract class AbstractPropertyDefinitions {

  private static final String PROP_PREFIX = "sonar.";
  private static final String EXTERNAL_ANALYZERS_CATEGORY = "External Analyzers";

  private final String languageKey;
  private final String languageName;
  private final String fileSuffixDefaultValue;
  private final SonarRuntime runtime;

  public AbstractPropertyDefinitions(String languageKey, String languageName, String fileSuffixDefaultValue, SonarRuntime runtime) {
    this.languageKey = languageKey;
    this.languageName = languageName;
    this.fileSuffixDefaultValue = fileSuffixDefaultValue;
    this.runtime = runtime;
  }

  public List<PropertyDefinition> create() {
    List<PropertyDefinition> result = new ArrayList<>();
    result.add(
      PropertyDefinition.builder(getRoslynJsonReportPathProperty(languageKey))
        .multiValues(true)
        .hidden()
        .build());

    result.add(
      PropertyDefinition.builder(getAnalyzerWorkDirProperty(languageKey))
        .multiValues(true)
        .hidden()
        .build());

    result.add(
      PropertyDefinition.builder(getFileSuffixProperty(languageKey))
        .category(languageName)
        .defaultValue(fileSuffixDefaultValue)
        .name("File suffixes")
        .description("Comma-separated list of suffixes of files to analyze.")
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

    if (runtime.getApiVersion().isGreaterThanOrEqual(Version.create(7, 4))) {
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
    }

    return result;
  }

  public static String getIgnoreHeaderCommentsProperty(String languageKey) {
    return PROP_PREFIX + languageKey + ".ignoreHeaderComments";
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
}
