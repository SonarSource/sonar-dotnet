/*
 * SonarVB
 * Copyright (C) 2012-2018 SonarSource SA
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

import org.sonar.api.rules.RuleFinder;
import org.sonarsource.dotnet.shared.plugins.AbstractSonarLintProfileExporter;

public class SonarLintProfileExporter extends AbstractSonarLintProfileExporter {
  private static final String PROFILE_KEY = "sonarlint-vs-vbnet";
  private static final String PROFILE_NAME = "SonarLint for Visual Studio Rule Set";

  public SonarLintProfileExporter(RuleFinder ruleFinder) {
    super(PROFILE_KEY, PROFILE_NAME, VbNetPlugin.LANGUAGE_KEY, VbNetPlugin.SONARANALYZER_NAME, VbNetPlugin.REPOSITORY_KEY, ruleFinder);
  }
}
