/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.sonar.api.Plugin;
import org.sonarsource.csharp.core.CSharpCoreExtensions;
import org.sonarsource.csharp.core.CSharpCorePluginMetadata;

public class CSharpPlugin implements Plugin {

  // Do NOT add any public fields here, and do NOT reference them directly. Add them to PluginMetadata and inject the metadata.
  static final CSharpCorePluginMetadata METADATA = new CSharpPluginMetadata();

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
  }
}
