/*
 * SonarSource :: .NET :: Shared library
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
package org.sonarsource.dotnet.shared.sarif;

import java.util.Collection;
import javax.annotation.Nullable;
import org.sonar.api.scanner.fs.InputProject;

public interface SarifParserCallback {

  void onProjectIssue(String ruleId, @Nullable String level, InputProject inputProject, String message);

  void onFileIssue(String ruleId, @Nullable String level, String absolutePath, Collection<Location> secondaryLocations, String message);

  void onIssue(String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations);

  void onRule(String ruleId, @Nullable String shortDescription, @Nullable String fullDescription, String defaultLevel, @Nullable String category);
}
