/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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

import java.util.Arrays;
import java.util.List;
import org.sonar.api.PropertyType;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;

public abstract class AbstractPropertyDefinitions {
  private static final String PROP_PREFIX = "sonar.";

  private final String languageKey;
  private final String fileSuffixDefaultValue;

  public AbstractPropertyDefinitions(String languageKey, String fileSuffixDefaultValue) {
    this.languageKey = languageKey;
    this.fileSuffixDefaultValue = fileSuffixDefaultValue;
  }

  public List<PropertyDefinition> create() {
    return Arrays.asList(
      PropertyDefinition.builder(getRoslynJsonReportPathProperty())
        .multiValues(true)
        .hidden()
        .build(),

      PropertyDefinition.builder(getAnalyzerWorkDirProperty())
        .multiValues(true)
        .hidden()
        .build(),

      PropertyDefinition.builder(PROP_PREFIX + languageKey + ".file.suffixes")
        .defaultValue(fileSuffixDefaultValue)
        .name("File suffixes")
        .description("Comma-separated list of suffixes of files to analyze.")
        .multiValues(true)
        .onQualifiers(Qualifiers.PROJECT)
        .build(),

      PropertyDefinition.builder(PROP_PREFIX + languageKey + ".ignoreHeaderComments")
        .defaultValue("true")
        .name("Ignore header comments")
        .description("If set to \"true\", the file headers (that are usually the same on each file: " +
          "licensing information for example) are not considered as comments. Thus metrics such as \"Comment lines\" " +
          "do not get incremented. If set to \"false\", those file headers are considered as comments and metrics such as " +
          "\"Comment lines\" get incremented.")
        .onQualifiers(Qualifiers.PROJECT)
        .type(PropertyType.BOOLEAN)
        .build());
  }

  private String getRoslynJsonReportPathProperty() {
    return PROP_PREFIX + languageKey + ".roslyn.reportFilePaths";
  }

  private String getAnalyzerWorkDirProperty() {
    return PROP_PREFIX + languageKey + ".analyzer.projectOutPaths";
  }
}
