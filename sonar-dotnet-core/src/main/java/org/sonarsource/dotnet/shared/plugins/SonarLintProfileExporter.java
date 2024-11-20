/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import org.sonar.api.rules.RuleFinder;

public class SonarLintProfileExporter extends AbstractSonarLintProfileExporter {
  private static final String PROFILE_NAME = "SonarLint for Visual Studio Rule Set";

  public SonarLintProfileExporter(RuleFinder ruleFinder, PluginMetadata pluginMetadata) {
    super("sonarlint-vs-" + pluginMetadata.languageKey(), PROFILE_NAME, pluginMetadata.languageKey(),
      pluginMetadata.analyzerProjectName(), pluginMetadata.repositoryKey(), ruleFinder);
  }
}
