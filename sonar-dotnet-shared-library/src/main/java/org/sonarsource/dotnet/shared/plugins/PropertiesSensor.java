/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
import java.util.stream.Collectors;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.scanner.ScannerSide;

/**
 * This class is a non-global sensor used to collect all Roslyn reports and all protobufs resulting from the analysis of each and every C#/VB.NET project.
 *
 * Note that this is required because a global sensor cannot access to module specific properties.
 */
@ScannerSide
public class PropertiesSensor implements Sensor {
  private final AbstractProjectConfiguration configuration;
  private final ReportPathCollector reportPathCollector;
  private final DotNetPluginMetadata pluginMetadata;

  public PropertiesSensor(AbstractProjectConfiguration configuration, ReportPathCollector reportPathCollector, DotNetPluginMetadata pluginMetadata) {
    this.configuration = configuration;
    this.reportPathCollector = reportPathCollector;
    this.pluginMetadata = pluginMetadata;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(pluginMetadata.shortLanguageName() + " Properties")
      .onlyOnLanguage(pluginMetadata.languageKey());
  }

  @Override
  public void execute(SensorContext context) {
    List<Path> protobufReportPaths = configuration.protobufReportPaths();
    if (!protobufReportPaths.isEmpty()) {
      reportPathCollector.addProtobufDirs(protobufReportPaths);
    }

    List<Path> roslynReportPaths = configuration.roslynReportPaths();
    if (!roslynReportPaths.isEmpty()) {
      reportPathCollector.addRoslynDirs(roslynReportPaths.stream().map(path -> new RoslynReport(context.project(), path)).collect(Collectors.toList()));
    }
  }
}
