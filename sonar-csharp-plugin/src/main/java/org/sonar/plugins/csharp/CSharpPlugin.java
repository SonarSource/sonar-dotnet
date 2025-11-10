/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.sonar.api.Plugin;
import org.sonarsource.csharp.core.CSharpCoreExtensions;
import org.sonarsource.csharp.core.CSharpCorePluginMetadata;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public class CSharpPlugin implements Plugin {

  // Do NOT add any public fields here, and do NOT reference them directly. Add them to PluginMetadata and inject the metadata.
  static final PluginMetadata METADATA = new CSharpPluginMetadata();

  @Override
  public void define(Context context) {
    CSharpCoreExtensions.register(context, METADATA);
  }

  private static class CSharpPluginMetadata extends CSharpCorePluginMetadata {

    @Override
    public String pluginKey() {
      return "csharp";
    }

    @Override
    public String analyzerProjectName() {
      return "SonarAnalyzer.CSharp";
    }

    @Override
    public String resourcesDirectory() {
      return "/org/sonar/plugins/csharp";
    }
  }
}
