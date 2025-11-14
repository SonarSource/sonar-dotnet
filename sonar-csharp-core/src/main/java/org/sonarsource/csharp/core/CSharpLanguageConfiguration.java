/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

import org.sonar.api.config.Configuration;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public class CSharpLanguageConfiguration extends AbstractLanguageConfiguration {
  public CSharpLanguageConfiguration(Configuration configuration, PluginMetadata metadata) {
    super(configuration, metadata);
  }

  public boolean analyzeRazorCode() {
    return configuration.getBoolean(CSharpPropertyDefinitions.getAnalyzeRazorCode(metadata.languageKey())).orElse(true);
  }
}
