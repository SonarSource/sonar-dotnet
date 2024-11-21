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

import java.nio.file.Path;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonarsource.dotnet.shared.plugins.protobuf.TelemetryImporter;

import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.TELEMETRY_FILENAME;

public class TelemetrySensor implements Sensor {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetrySensor.class);
  private final PluginMetadata pluginMetadata;
  private final ModuleConfiguration configuration;

  public TelemetrySensor(PluginMetadata pluginMetadata, ModuleConfiguration configuration) {
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Telemetry", pluginMetadata.languageName());
    descriptor.name(name).onlyOnLanguage(pluginMetadata.languageKey());
  }

  @Override
  public void execute(SensorContext context) {
    LOG.debug("Start importing metrics.");
    TelemetryImporter importer = new TelemetryImporter(context, pluginMetadata.pluginKey(), pluginMetadata.languageKey());
    for (Path protobufDir : configuration.protobufReportPaths()) {
      ProtobufDataImporter.parseProtobuf(importer, protobufDir, TELEMETRY_FILENAME);
    }
    LOG.debug("Start adding metrics.");
    importer.save();
    LOG.debug("Finished adding metrics.");
  }
}
