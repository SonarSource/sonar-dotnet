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

/**
 * The TelemetryJsonProcessor is executed once per project (sln) and plugin (vb and c#) after all module sensors (TelemetryJsonSensor) are run.
 * It takes the TelemetryJsonCollector, which contains all telemetry found by the module sensors (.\sonarqube\out\0\Telemetry.json).
 * In execute, this TelemetryJsonProcessor first executes TelemetryJsonProjectCollector. TelemetryJsonProjectCollector adds the global
 * .\sonarqube\out\Telemetry.*.json to the TelemetryJsonCollector. This is done in the C# plugin only because otherwise, such telemetry would be processed twice.
 * TelemetryJsonProjectCollector.Impl is registered in the C# plugin as TelemetryJsonProjectCollector.
 * TelemetryJsonProjectCollector.Empty is registered in the VB plugin as TelemetryJsonProjectCollector.
 * This means TelemetryJsonCollector is filled up this way:
 * TelemetryJsonCollector in the C# plugin:
 * 1. csproj specific telemetry is collected by TelemetryJsonSensor
 * 2. TelemetryJsonProjectCollector.Impl adds the project wide scanner telemetry (from e.g. the begin step and the end step)
 * TelemetryJsonCollector in the VB plugin
 * 1. vbproj specific telemetry is collected by TelemetryJsonSensor
 * 2. TelemetryJsonProjectCollector.Empty does not add anymore telemetry because that telemetry is already processed by the C# plugin
 * All telemetry in TelemetryJsonCollector is then aggregated and send to the server (this happens in the C# plugin and the VB plugin).
 */
public class TelemetryJsonProcessor implements ProjectSensor {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetryJsonProcessor.class);
  private final TelemetryJsonProjectCollector projectSensor;
  private final PluginMetadata pluginMetadata;
  private final TelemetryJsonCollector collector;

  public TelemetryJsonProcessor(TelemetryJsonCollector collector, TelemetryJsonProjectCollector projectSensor, PluginMetadata pluginMetadata) {
    this.collector = collector;
    this.projectSensor = projectSensor;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor sensorDescriptor) {
    String name = String.format("%s Telemetry Json processor", pluginMetadata.languageName());
    sensorDescriptor.name(name);
  }

  @Override
  public void execute(@Nonnull SensorContext sensorContext) {
    projectSensor.execute(sensorContext);

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
