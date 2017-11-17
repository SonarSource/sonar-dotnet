/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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
import java.util.Optional;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;

@ScannerSide
public abstract class AbstractPropertiesSensor implements Sensor {
  private final AbstractConfiguration configuration;
  private final ReportPathCollector reportPathCollector;
  private final String sensorName;
  private final String languageKey;

  public AbstractPropertiesSensor(AbstractConfiguration configuration, ReportPathCollector reportPathCollector, String sensorName, String languageKey) {
    this.configuration = configuration;
    this.reportPathCollector = reportPathCollector;
    this.sensorName = sensorName;
    this.languageKey = languageKey;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(sensorName)
      .onlyOnLanguage(languageKey);
  }

  @Override
  public void execute(SensorContext context) {
    Optional<Path> protobufReportPath = configuration.protobufReportPath();
    if (protobufReportPath.isPresent()) {
      reportPathCollector.addProtobufDir(protobufReportPath.get());
    }

    Optional<Path> roslynReportPath = configuration.roslynReportPath();
    if (roslynReportPath.isPresent()) {
      reportPathCollector.addRoslynDir(roslynReportPath.get());
    }
  }
}
