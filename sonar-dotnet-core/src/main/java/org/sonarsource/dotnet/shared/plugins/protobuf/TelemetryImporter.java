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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.shared.plugins.TelemetryCollector;

public class TelemetryImporter extends RawProtobufImporter<SonarAnalyzer.Telemetry> {
  private final TelemetryCollector collector;

  public TelemetryImporter(TelemetryCollector collector) {
    super(SonarAnalyzer.Telemetry.parser());
    this.collector = collector;
  }

  @Override
  void consume(SonarAnalyzer.Telemetry message) {
    collector.addTelemetry(message);
  }
}
