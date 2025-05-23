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

import org.sonar.api.config.Configuration;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;

public class CSharpLanguageConfiguration extends AbstractLanguageConfiguration {
  public CSharpLanguageConfiguration(Configuration configuration) {
    super(configuration, CSharpPlugin.LANGUAGE_KEY);
  }

  public boolean analyzeRazorCode() {
    return configuration.getBoolean(CSharpPropertyDefinitions.getAnalyzeRazorCode(CSharpPlugin.LANGUAGE_KEY)).orElse(true);
  }
}
