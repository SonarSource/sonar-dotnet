/*
 * SonarSource :: C# :: Core
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
        .description("If set to \"true\", .razor and .cshtml files will be fully analysed, this may increase the analysis time." +
          " If set to \"false\", .cshtml files will be analysed for taint vulnerabilities only.")
        .onQualifiers(Qualifiers.PROJECT)
        .type(PropertyType.BOOLEAN)
        .build());
    return result;
  }

  public static String getAnalyzeRazorCode(String languageKey) {
    return PROP_PREFIX + languageKey + ".analyzeRazorCode";
  }
}
