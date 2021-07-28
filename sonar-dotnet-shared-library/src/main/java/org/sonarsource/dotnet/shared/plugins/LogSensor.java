/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins;

import java.nio.file.Path;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.LogImporter;

import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.LOG_FILENAME;

public class LogSensor implements Sensor {
  private static final Logger LOG = Loggers.get(LogSensor.class);
  private final DotNetPluginMetadata pluginMetadata;
  private final AbstractModuleConfiguration configuration;

  public LogSensor(DotNetPluginMetadata pluginMetadata, AbstractModuleConfiguration configuration) {
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    // We don't filter by language to be invoked on projects without sources - when referencing shared project
    String name = String.format("%s Analysis Log", pluginMetadata.shortLanguageName());
    descriptor.name(name);
  }

  @Override
  public void execute(SensorContext context) {
    LogImporter importer = new LogImporter(LOG);
    for (Path protobufDir : configuration.protobufReportPaths()) {
      ProtobufDataImporter.parseProtobuf(importer, protobufDir, LOG_FILENAME);
      importer.save();
    }
  }
}
