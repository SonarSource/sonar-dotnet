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

import java.nio.file.Path;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.TelemetryCollector;
import org.sonarsource.dotnet.shared.plugins.protobuf.TelemetryImporter;

import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.TELEMETRY_FILENAME;

/**
 * This sensor is run once per csproj or vbproj build. It reads the protobuf files
 * associated with the project (can be more than one in case of multitargeting via &lt;TargetFrameworks&gt;)
 * The telemetry messages are added to the scanner run wide TelemetryCollector and later post-processed by TelemetryProcessor.
 */
public class TelemetrySensor implements Sensor {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetrySensor.class);
  private final PluginMetadata pluginMetadata;
  private final ModuleConfiguration configuration;
  private final TelemetryCollector collector;

  public TelemetrySensor(TelemetryCollector collector, PluginMetadata pluginMetadata, ModuleConfiguration configuration) {
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
    this.collector = collector;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Telemetry", pluginMetadata.languageName());
    descriptor.name(name).onlyOnLanguage(pluginMetadata.languageKey());
  }

  @Override
  public void execute(SensorContext context) {
    LOG.debug("Start importing metrics.");
    TelemetryImporter importer = new TelemetryImporter(collector);
    for (Path protobufDir : configuration.protobufReportPaths()) {
      ProtobufDataImporter.parseProtobuf(importer, protobufDir, TELEMETRY_FILENAME);
    }
  }
}
