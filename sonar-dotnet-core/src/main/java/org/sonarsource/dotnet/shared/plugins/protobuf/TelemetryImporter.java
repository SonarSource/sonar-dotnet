/*
 * SonarSource :: .NET :: Core
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.util.ArrayList;
import java.util.List;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

public class TelemetryImporter extends RawProtobufImporter<SonarAnalyzer.Telemetry> {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetryImporter.class);
  private final SensorContext context;
  private final List<SonarAnalyzer.Telemetry> messages = new ArrayList<>();
  private final TelemetryAggregator aggregator;

  public TelemetryImporter(SensorContext context, String pluginKey, String language) {
    super(SonarAnalyzer.Telemetry.parser());
    this.context = context;
    this.aggregator = new TelemetryAggregator(pluginKey, language);
  }

  @Override
  void consume(SonarAnalyzer.Telemetry message) {
    messages.add(message);
  }

  @Override
  public void save() {
    LOG.debug("Found telemetry for {} projects.", messages.size());
    var telemetries = aggregator.aggregate(messages);
    LOG.debug("Aggregated {} telemetry messages.", telemetries.size());
    telemetries.forEach(telemetry -> context.addTelemetryProperty(telemetry.getKey(), telemetry.getValue()));
    LOG.debug("Send {} telemetry messages.", telemetries.size());
  }
}
