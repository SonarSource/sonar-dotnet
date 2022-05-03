/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2022 SonarSource SA
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

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.config.Configuration;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Type;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.regex.Pattern;
import java.util.stream.Stream;

public class AnalysisWarningsSensor implements Sensor {

  private static final Logger LOG = Loggers.get(AnalysisWarningsSensor.class);
  private static final String SUFFIX = ".sonar";
  private static final Gson GSON = new Gson();

  private static final Pattern AnalysisWarningsPattern = Pattern.compile("AnalysisWarnings\\..*\\.json");
  private final DotNetPluginMetadata metadata;
  private final Configuration configuration;
  private final AnalysisWarnings analysisWarnings;

  public AnalysisWarningsSensor(Configuration configuration, DotNetPluginMetadata metadata, AnalysisWarnings analysisWarnings){
    this.configuration = configuration;
    this.metadata = metadata;
    this.analysisWarnings = analysisWarnings;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    String name = String.format("%s Analysis Warnings import", metadata.shortLanguageName());
    descriptor.name(name);
  }

  @Override
  public void execute(SensorContext sensorContext) {
    // Working directory folder is constructed from SonarOutputDir + ".sonar". We have to remove the suffix and search for valid configuration files.
    // e.g.
    //    .sonarqube\out\AnalysisWarnings.AutoScan.json
    //    .sonarqube\out\AnalysisWarnings.Scanner.json
    configuration
      .get("sonar.working.directory")
      .filter(s -> s.endsWith(SUFFIX))
      .map(AnalysisWarningsSensor::getOutputDir)
      .map(AnalysisWarningsSensor::getFilePaths)
      .ifPresent(this::publishMessages);
  }

  private static Path getOutputDir(String workingDirectory) {
    return Paths.get(workingDirectory).getParent();
  }

  private static Stream<Path> getFilePaths(Path outputDirectory) {
    try {
      return Files.find(outputDirectory, 1, (path, attributes) -> AnalysisWarningsPattern.matcher(path.toFile().getName()).matches());
    } catch (IOException exception) {
      LOG.warn("Error occurred while loading analysis analysis warnings", exception);
      return Stream.empty();
    }
  }

  private void publishMessages(Stream<Path> paths) {
    Type collectionType = new TypeToken<List<Warning>>(){}.getType();
    paths.forEach(path -> {
      try (InputStream is = Files.newInputStream(path)) {
        List<Warning> warnings = GSON.fromJson(new InputStreamReader(is, StandardCharsets.UTF_8), collectionType);
        warnings.forEach(message -> analysisWarnings.addUnique(message.getText()));
      } catch (Exception exception) {
        LOG.error("Error occurred while publishing analysis warnings", exception);
      }
    });
  }

  private static class Warning {
    private String text = "";

    public String getText() {
      return text;
    }
  }
}
