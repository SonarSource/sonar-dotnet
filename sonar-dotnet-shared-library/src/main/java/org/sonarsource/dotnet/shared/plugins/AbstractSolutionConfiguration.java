/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.config.Configuration;

/**
 * This configuration is at the level of the solution ("project" in scanner-cli terminology).
 *
 * This class is consumed by the GeneratedFileFilter, thus needs to be at solution (scanner "project") level.
 */
@ScannerSide
public abstract class AbstractSolutionConfiguration {

  private final Configuration configuration;
  private final String languageKey;

  public AbstractSolutionConfiguration(Configuration configuration, String languageKey) {
    this.configuration = configuration;
    this.languageKey = languageKey;
  }

  public boolean analyzeGeneratedCode() {
    return configuration.getBoolean(AbstractPropertyDefinitions.getAnalyzeGeneratedCode(languageKey)).orElse(false);
  }
}
