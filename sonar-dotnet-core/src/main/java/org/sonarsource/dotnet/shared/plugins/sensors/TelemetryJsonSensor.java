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

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import javax.annotation.Nonnull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonParser;

public class TelemetryJsonSensor implements Sensor {

  private static final Logger LOG = LoggerFactory.getLogger(TelemetryJsonSensor.class);
  private final PluginMetadata pluginMetadata;
  private final ModuleConfiguration configuration;
  private final TelemetryJsonCollector collector;

  public TelemetryJsonSensor(TelemetryJsonCollector collector, PluginMetadata pluginMetadata, ModuleConfiguration configuration) {
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
    this.collector = collector;
  }

  @Override
  public void describe(SensorDescriptor sensorDescriptor) {
    String name = String.format("%s Telemetry Json", pluginMetadata.languageName());
    sensorDescriptor.name(name).onlyOnLanguage(pluginMetadata.languageKey());
  }

  @Override
  public void execute(@Nonnull SensorContext sensorContext) {
    var parser = new TelemetryJsonParser();
    for (var file : configuration.telemetryJsonPaths()) {
      try (
        var input = new FileInputStream(file.toFile());
        var reader = new InputStreamReader(input, StandardCharsets.UTF_8)) {
        parser.parse(reader).forEach(collector::addTelemetry);
      } catch (IOException e) {
        LOG.debug("Cannot open telemetry file {}, {}", file, e.toString());
      }
    }
  }
}
