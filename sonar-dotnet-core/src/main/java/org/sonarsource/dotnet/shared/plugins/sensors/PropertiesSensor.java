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
import java.util.List;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynReport;

/**
 * This class is a non-global sensor used to collect all Roslyn reports and all protobufs resulting from the analysis of each and every C#/VB.NET project.
 *
 * Note that this is required because a global sensor cannot access to module specific properties.
 */
public class PropertiesSensor implements Sensor {
  private final ModuleConfiguration configuration;
  private final ReportPathCollector reportPathCollector;
  private final PluginMetadata pluginMetadata;

  public PropertiesSensor(ModuleConfiguration configuration, ReportPathCollector reportPathCollector,
    PluginMetadata pluginMetadata) {
    this.configuration = configuration;
    this.reportPathCollector = reportPathCollector;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(pluginMetadata.languageName() + " Properties");
    // we do not filter by language because we want to be called on projects without sources
    // (that could reference only shared sources e.g. in .projitems)
  }

  @Override
  public void execute(SensorContext context) {
    List<Path> protobufReportPaths = configuration.protobufReportPaths();
    if (!protobufReportPaths.isEmpty()) {
      reportPathCollector.addProtobufDirs(protobufReportPaths);
    }

    List<Path> roslynReportPaths = configuration.roslynReportPaths();
    if (!roslynReportPaths.isEmpty()) {
      reportPathCollector.addRoslynReport(roslynReportPaths.stream().map(path -> new RoslynReport(context.project(), path)).toList());
    }
  }
}
