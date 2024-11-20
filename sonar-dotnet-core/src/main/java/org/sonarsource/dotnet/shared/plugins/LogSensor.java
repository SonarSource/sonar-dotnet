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
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonarsource.dotnet.shared.plugins.protobuf.LogImporter;

import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.LOG_FILENAME;

public class LogSensor implements Sensor {
  private final PluginMetadata pluginMetadata;
  private final ModuleConfiguration configuration;

  public LogSensor(PluginMetadata pluginMetadata, ModuleConfiguration configuration) {
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    // We don't filter by language to be invoked on projects without sources - when referencing shared project
    String name = String.format("%s Analysis Log", pluginMetadata.languageName());
    descriptor.name(name);
  }

  @Override
  public void execute(SensorContext context) {
    LogImporter importer = new LogImporter();
    for (Path protobufDir : configuration.protobufReportPaths()) {
      ProtobufDataImporter.parseProtobuf(importer, protobufDir, LOG_FILENAME);
      importer.save();
    }
  }
}
