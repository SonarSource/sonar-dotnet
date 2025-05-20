/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins.sensors;

import javax.annotation.Nonnull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;

public class TelemetryJsonProcessor implements ProjectSensor {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetryJsonProcessor.class);
  private final PluginMetadata pluginMetadata;
  private final TelemetryJsonCollector collector;

  public TelemetryJsonProcessor(TelemetryJsonCollector collector, PluginMetadata pluginMetadata) {
    this.collector = collector;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor sensorDescriptor) {
    String name = String.format("%s Telemetry Json processor", pluginMetadata.languageName());
    sensorDescriptor.name(name);
  }

  @Override
  public void execute(@Nonnull SensorContext sensorContext) {
    if (collector == null) {
      LOG.debug("TelemetryJsonCollector is null, skipping telemetry processing.");
      return;
    }
    var messages = collector.getTelemetry();
    LOG.debug("Found {} telemetry messages.", messages.size());
    messages.forEach(telemetry -> {
      LOG.debug("Adding metric: {}={}", telemetry.getKey(), telemetry.getValue());
      sensorContext.addTelemetryProperty(telemetry.getKey(), telemetry.getValue());
    });
    LOG.debug("Added {} metrics.", messages.size());
  }
}
