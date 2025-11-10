/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonarsource.csharp.core;

import java.util.List;
import org.sonar.api.PropertyType;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;
import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public class CSharpPropertyDefinitions extends AbstractPropertyDefinitions {

  public CSharpPropertyDefinitions(PluginMetadata metadata) {
    super(metadata);
  }

  @Override
  public List<PropertyDefinition> create() {
    List<PropertyDefinition> result = super.create();
    result.add(
      PropertyDefinition.builder(getAnalyzeRazorCode(metadata.languageKey()))
        .category(metadata.languageName())
        .defaultValue("true")
        .name("Analyze Razor code")
        .description("If set to \"true\", .razor and .cshtml files will be fully analyzed, this may increase the analysis time." +
          " If set to \"false\", .cshtml files will be analyzed for taint vulnerabilities only.")
        .onQualifiers(Qualifiers.PROJECT)
        .type(PropertyType.BOOLEAN)
        .build());
    return result;
  }

  public static String getAnalyzeRazorCode(String languageKey) {
    return PROP_PREFIX + languageKey + ".analyzeRazorCode";
  }
}
