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

import java.util.ArrayList;
import java.util.Collection;
import java.util.Map;
import org.sonar.api.scanner.ScannerSide;

@ScannerSide
public class TelemetryJsonCollector {
  private final ArrayList<Map.Entry<String, String>> telemetry;

  public TelemetryJsonCollector() {
    this.telemetry = new ArrayList<>();
  }

  public void addTelemetry(String key, String value) {
    telemetry.add(Map.entry(key, value));
  }

  public void addTelemetry(Map.Entry<String, String> telemetry) {
    this.telemetry.add(telemetry);
  }

  public Collection<Map.Entry<String, String>> getTelemetry() {
    return telemetry;
  }
}
