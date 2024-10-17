/*
 * SonarVB
 * Copyright (C) 2012-2024 SonarSource SA
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
