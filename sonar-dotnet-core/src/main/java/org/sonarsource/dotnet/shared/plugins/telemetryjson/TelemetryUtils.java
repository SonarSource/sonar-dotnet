/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins.telemetryjson;

import java.util.Locale;
import java.util.regex.Pattern;

public class TelemetryUtils {
  // Replace any non-word-character and non-digit with "_"
  private static final Pattern sanitizeKeyPattern = Pattern.compile("[^a-zA-Z0-9]");

  private TelemetryUtils() {
    // Private constructor to hide the implicit public one
  }

  public static String sanitizeKey(String x) {
    return sanitizeKeyPattern.matcher(x).replaceAll("_").toLowerCase(Locale.ROOT);
  }

}
