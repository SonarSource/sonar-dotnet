/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
import java.util.List;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;

/**
 * This class is a non-global sensor used to collect all Roslyn reports and all protobufs resulting from the analysis of each and every C#/VB.NET project.
 *
 * Note that this is required because a global sensor cannot access to module specific properties.
 */
public class PropertiesSensor implements Sensor {
  private final AbstractModuleConfiguration configuration;
  private final ReportPathCollector reportPathCollector;
  private final DotNetPluginMetadata pluginMetadata;

  public PropertiesSensor(AbstractModuleConfiguration configuration, ReportPathCollector reportPathCollector, DotNetPluginMetadata pluginMetadata) {
    this.configuration = configuration;
    this.reportPathCollector = reportPathCollector;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(pluginMetadata.shortLanguageName() + " Properties");
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
