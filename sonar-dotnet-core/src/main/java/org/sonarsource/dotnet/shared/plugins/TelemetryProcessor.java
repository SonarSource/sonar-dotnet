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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonarsource.dotnet.shared.plugins.protobuf.TelemetryAggregator;

/**
 * This sensor is run once per run and after all TelemetrySensor are executed.
 * It takes the telemetry messages from the single sensors, aggregates them and adds
 * them to the telemetry.
 */
public class TelemetryProcessor implements ProjectSensor {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetryProcessor.class);
  private final PluginMetadata pluginMetadata;
  private final TelemetryCollector collector;

  public TelemetryProcessor(TelemetryCollector collector, PluginMetadata pluginMetadata) {
    this.pluginMetadata = pluginMetadata;
    this.collector = collector;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Telemetry processor", pluginMetadata.languageName());
    descriptor.name(name);
  }

  @Override
  public void execute(SensorContext context) {
    if (collector == null) {
      LOG.debug("TelemetryCollector is null, skipping telemetry processing.");
      return;
    }
    var aggregator = new TelemetryAggregator(pluginMetadata.pluginKey(), pluginMetadata.languageKey());
    var messages = collector.getTelemetryMessages();
    LOG.debug("Found {} telemetry messages reported by the analyzers.", messages.size());
    var telemetries = aggregator.aggregate(messages);
    LOG.debug("Aggregated {} metrics.", telemetries.size());
    telemetries.forEach(telemetry -> {
      LOG.debug("Adding metric: {}={}", telemetry.getKey(), telemetry.getValue());
      context.addTelemetryProperty(telemetry.getKey(), telemetry.getValue());
    });
    LOG.debug("Added {} metrics.", telemetries.size());
  }
}
