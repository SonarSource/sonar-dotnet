/*
 * SonarVB
 * Copyright (C) 2012-2025 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.sonar.api.Plugin;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.vbnet.core.VbNetCoreExtensions;
import org.sonarsource.vbnet.core.VbNetCorePluginMetadata;

public class VbNetPlugin implements Plugin {

  // Do NOT add any public fields here, and do NOT reference them directly. Add them to PluginMetadata and inject the metadata.
  static final PluginMetadata METADATA = new VbNetPluginMetadata();

  @Override
  public void define(Context context) {
    VbNetCoreExtensions.register(context, METADATA);
  }

  private static class VbNetPluginMetadata extends VbNetCorePluginMetadata {

    @Override
    public String pluginKey() {
      return "vbnet";
    }

    @Override
    public String analyzerProjectName() {
      return "SonarAnalyzer.VisualBasic";
    }

    @Override
    public String resourcesDirectory() {
      return "/org/sonar/plugins/vbnet";
    }
  }
}
