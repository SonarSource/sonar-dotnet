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

import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public abstract class CSharpCorePluginMetadata implements PluginMetadata {

  @Override
  public String languageKey() {
    return "cs";
  }

  @Override
  public String languageName() {
    return "C#";
  }

  @Override
  public String repositoryKey() {
    return "csharpsquid";
  }

  @Override
  public String fileSuffixesKey() {
    return AbstractPropertyDefinitions.getFileSuffixProperty(languageKey());
  }

  @Override
  public String fileSuffixesDefaultValue() {
    return ".cs,.razor";
  }

  @Override
  public String resourcesDirectory() {
    return "/org/sonar/plugins/csharp";
  }
}
