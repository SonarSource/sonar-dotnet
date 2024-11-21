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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.util.ArrayList;
import java.util.List;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

public class TelemetryImporter extends RawProtobufImporter<SonarAnalyzer.Telemetry> {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetryImporter.class);
  private final SensorContext context;
  private final List<SonarAnalyzer.Telemetry> messages = new ArrayList<>();
  private final TelemetryAggregator aggregator;

  public TelemetryImporter(SensorContext context, String pluginKey, String language) {
    super(SonarAnalyzer.Telemetry.parser());
    this.context = context;
    this.aggregator = new TelemetryAggregator(pluginKey, language);
  }

  @Override
  void consume(SonarAnalyzer.Telemetry message) {
    messages.add(message);
  }

  @Override
  public void save() {
    LOG.debug("Found metrics for {} projects.", messages.size());
    var telemetries = aggregator.aggregate(messages);
    LOG.debug("Aggregated {} metric messages.", telemetries.size());
    telemetries.forEach(telemetry -> {
      LOG.debug("Adding metric: {}={}", telemetry.getKey(), telemetry.getValue());
      context.addTelemetryProperty(telemetry.getKey(), telemetry.getValue());
    });
    LOG.debug("Added {} metrics.", telemetries.size());
  }
}
