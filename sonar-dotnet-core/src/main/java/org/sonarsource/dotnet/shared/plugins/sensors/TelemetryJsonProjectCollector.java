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
import java.util.Optional;
import java.util.regex.Pattern;
import java.util.stream.Stream;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonParser;

/**
 * This class is a dependency of TelemetryJsonProcessor. TelemetryJsonProcessor is instantiated ones per plugin per project.
 * This class is responsible for adding telemetry from .\sonarqube\out\Telemetry.*.json to TelemetryJsonCollector.
 * There are two instances of this class. One registered for the VB plugin and one by the C# plugin. The first of the two plugins
 * that is executed wins, and "marks" the Telemetry files as processed by renaming them. That way, the telemetry is only added once.
 * The order of the plugins is non-deterministic, so either of the plugins can pick up the telemetry in a mixed solution.
 * TelemetryJsonProcessor takes all telemetry from TelemetryJsonCollector and sends it to the SQ server. This includes these telemetries:
 * * All module level telemetry collected by TelemetryJsonSensor (language specific)
 * * C# or VB plugin (whoever comes first): The global .\sonarqube\out\Telemetry.*.json telemetry collected by this class
 */
@ScannerSide
public class TelemetryJsonProjectCollector {
  private static final Logger LOG = LoggerFactory.getLogger(TelemetryJsonProjectCollector.class);
  private static final Pattern TelemetryPattern = Pattern.compile("^Telemetry\\..*\\.json$");
  private final TelemetryJsonCollector collector;
  private final AbstractLanguageConfiguration configuration;

  public TelemetryJsonProjectCollector(TelemetryJsonCollector collector, AbstractLanguageConfiguration configuration) {
    this.collector = collector;
    this.configuration = configuration;
  }

  public void execute() {
    configuration.outputDir()
      .map(TelemetryJsonProjectCollector::getFilePaths)
      .ifPresent(this::collectTelemetry);
  }

  private void collectTelemetry(Stream<Path> pathStream) {
    var parser = new TelemetryJsonParser();
    pathStream.forEach(file -> markAsProcessed(file).ifPresent(x ->
    {
      try (
        var input = new FileInputStream(x.toFile());
        var reader = new InputStreamReader(input, StandardCharsets.UTF_8)) {
        parser.parse(reader).forEach(collector::addTelemetry);
      } catch (IOException e) {
        LOG.debug("Cannot open telemetry file {}, {}", file, e.toString());
      }
    }));
  }

  /**
   * Marks the found telemetry file by renaming it. Returns the new file name or Empty in case of an error.
   * The rename assures, that any Telemetry file is processed only ones.
   * This is needed because this class is executed by the VB and the C# plugin but
   * the telemetry should only be imported once.
  */
  private static Optional<Path> markAsProcessed(Path file) {
    var newFile = file.resolveSibling("r" + file.getFileName());
    try {
      var result = Files.move(file, newFile);
      return Optional.ofNullable(result);
    } catch (IOException e) {
      return Optional.empty();
    }
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
