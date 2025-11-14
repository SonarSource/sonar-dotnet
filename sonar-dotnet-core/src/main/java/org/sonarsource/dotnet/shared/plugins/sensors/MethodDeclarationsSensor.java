/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
import org.sonarsource.dotnet.shared.plugins.MethodDeclarationsCollector;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.MethodDeclarationsImporter;

import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.METHODDECLARATIONS_FILENAME;

/**
 * This sensor is run once per csproj or vbproj build. It reads the protobuf files associated with the project
 * The method declarations are added to the scanner run wide MethodDeclarationsCollector.
 */
public class MethodDeclarationsSensor implements Sensor {
  private static final Logger LOG = LoggerFactory.getLogger(MethodDeclarationsSensor.class);
  private final PluginMetadata pluginMetadata;
  private final ModuleConfiguration configuration;
  private final MethodDeclarationsCollector collector;

  public MethodDeclarationsSensor(MethodDeclarationsCollector collector, PluginMetadata pluginMetadata, ModuleConfiguration configuration) {
    this.pluginMetadata = pluginMetadata;
    this.configuration = configuration;
    this.collector = collector;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Method Declarations", pluginMetadata.languageName());
    descriptor.name(name);
  }

  @Override
  public void execute(SensorContext context) {
    LOG.debug("Start importing method declarations.");
    MethodDeclarationsImporter importer = new MethodDeclarationsImporter(collector);
    for (Path protobufDir : configuration.protobufReportPaths()) {
      ProtobufDataImporter.parseProtobuf(importer, protobufDir, METHODDECLARATIONS_FILENAME);
    }
  }
}
