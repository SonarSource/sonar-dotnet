/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonarsource.dotnet.shared.plugins.telemetryjson;

public class TelemetryUtils {

  private TelemetryUtils() {
    // Private constructor to hide the implicit public one
  }

  // See https://github.com/SonarSource/sonar-scanner-msbuild/blob/master/src/SonarScanner.MSBuild.Common/Telemetry/TelemetryUtils.cs
  // See https://xtranet-sonarsource.atlassian.net/wiki/spaces/DP/pages/3912630334/SonarSource+Telemetry+System+Sending+and+Using+Measures#Naming-conventions
  public static String sanitizeKey(String x) {
    var sb = new StringBuilder(x.length());
    for (char c : x.toCharArray()) {
      sb.append(c < 128 && Character.isLetterOrDigit(c) ? Character.toLowerCase(c) : '_');
    }
    return sb.toString();
  }

}
