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
package org.sonarsource.dotnet.shared.sarif;

import java.util.Collection;
import javax.annotation.Nullable;
import org.sonar.api.scanner.fs.InputProject;

public interface SarifParserCallback {

  void onProjectIssue(String ruleId, @Nullable String level, InputProject inputProject, String message);

  void onFileIssue(String ruleId, @Nullable String level, String absolutePath, Collection<Location> secondaryLocations, String message);

  void onIssue(String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations, boolean withExecutionFlow);

  void onRule(String ruleId, @Nullable String shortDescription, @Nullable String fullDescription, String defaultLevel, @Nullable String category);
}
