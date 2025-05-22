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

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.regex.Pattern;
import java.util.stream.Stream;
import javax.annotation.Nonnull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonParser;

/**
 * This class is a dependency of TelemetryJsonProcessor. TelemetryJsonProcessor is instantiated ones per plugin per project.
 * This class is responsible for adding telemetry from .\sonarqube\out\Telemetry.*.json to TelemetryJsonCollector.
 * There are two implementations of this class: "Empty" is registered by the VB plugin and does nothing. "Impl" is registered by
 * the C# plugin and adds all telemetry from .\sonarqube\out\Telemetry.*.json to TelemetryJsonCollector.
 * TelemetryJsonProcessor takes all telemetry from TelemetryJsonCollector and sends it to the SQ server. This includes these telemetries:
 * * All module level telemetry collected by TelemetryJsonSensor (language specific)
 * * In the C# plugin only: The global .\sonarqube\out\Telemetry.*.json telemetry collected by TelemetryJsonProjectCollector.Impl
 */
@ScannerSide
public abstract class TelemetryJsonProjectCollector {
  public abstract void execute(@Nonnull SensorContext sensorContext);

  public static class Empty extends TelemetryJsonProjectCollector {
    @Override
    public void execute(@Nonnull SensorContext sensorContext) {
      // Do nothing. This class is registered in the VB plugin, while Impl is registered by the C# plugin. That way, the global Telemetry.*.json
      // file is only imported once.
    }
  }

  public static class Impl extends TelemetryJsonProjectCollector {

    private static final Logger LOG = LoggerFactory.getLogger(Impl.class);
    private static final Pattern TelemetryPattern = Pattern.compile("Telemetry\\..*\\.json");
    private final TelemetryJsonCollector collector;
    private final AbstractLanguageConfiguration configuration;

    public Impl(TelemetryJsonCollector collector, AbstractLanguageConfiguration configuration) {
      this.collector = collector;
      this.configuration = configuration;
    }

    @Override
    public void execute(@Nonnull SensorContext sensorContext) {
      configuration.outputDir()
        .map(Impl::getFilePaths)
        .ifPresent(this::collectTelemetry);
    }

    private void collectTelemetry(Stream<Path> pathStream) {
      var parser = new TelemetryJsonParser();
      pathStream.forEach(file -> {
        try (
          var input = new FileInputStream(file.toFile());
          var reader = new InputStreamReader(input, StandardCharsets.UTF_8)) {
          parser.parse(reader).forEach(collector::addTelemetry);
        } catch (IOException e) {
          LOG.debug("Cannot open telemetry file {}, {}", file, e.toString());
        }
      });
    }

    private static Stream<Path> getFilePaths(Path outputDirectory) {
      LOG.debug("Searching for telemetry json in {}", outputDirectory);
      try {
        return Files.find(outputDirectory, 1, (path, attributes) -> TelemetryPattern.matcher(path.toFile().getName()).matches());
      } catch (IOException exception) {
        LOG.warn("Error occurred while loading telemetry json", exception);
        return Stream.empty();
      }
    }
  }
}
