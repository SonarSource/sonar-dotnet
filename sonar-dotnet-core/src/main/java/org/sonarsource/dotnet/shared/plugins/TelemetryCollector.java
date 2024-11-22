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
package org.sonarsource.dotnet.shared.plugins;

import java.util.ArrayList;
import java.util.Collection;
import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

/**
 * A simple project wide data collector (created before all module sensors),
 * that allows TelemetrySensors to add telemetry messages.
 * It is injected to each TelemetrySensor (for appending messages) and to the project
 * wide TelemetryProcessor (for reading all messages, executed after all module sensors).
 */
@ScannerSide
public class TelemetryCollector {
  private final ArrayList<SonarAnalyzer.Telemetry> telemetryMessages;

  public TelemetryCollector() {
    this.telemetryMessages = new ArrayList<>();
  }

  public void addTelemetry(SonarAnalyzer.Telemetry telemetry) {
    this.telemetryMessages.add(telemetry);
  }

  public Collection<SonarAnalyzer.Telemetry> getTelemetryMessages() {
    return telemetryMessages;
  }
}
