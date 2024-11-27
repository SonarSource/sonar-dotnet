/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2024 SonarSource SA
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
package org.sonarsource.vbnet.core;

import org.sonar.api.Plugin.Context;
import org.sonarsource.dotnet.shared.plugins.CodeCoverageProvider;
import org.sonarsource.dotnet.shared.plugins.DotNetRulesDefinition;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.GlobalProtobufFileProcessor;
import org.sonarsource.dotnet.shared.plugins.HashProvider;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.ProjectTypeCollector;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynProfileExporter;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;
import org.sonarsource.dotnet.shared.plugins.SonarLintProfileExporter;
import org.sonarsource.dotnet.shared.plugins.TelemetryCollector;
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;
import org.sonarsource.dotnet.shared.plugins.sensors.DotNetSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.FileTypeSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.LogSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.PropertiesSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryProcessor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetrySensor;

public class VbNetCoreExtensions {

  private VbNetCoreExtensions() {
    // Private constructor to prevent instantiation
  }

  public static void register(Context context, PluginMetadata metadata) {
    context.addExtensions(
      // module-level components (some relying on deprecated Scanner APIs)
      FileTypeSensor.class,
      LogSensor.class,
      TelemetryCollector.class,
      TelemetrySensor.class,
      TelemetryProcessor.class,
      PropertiesSensor.class,
      ModuleConfiguration.class,
      // global components
      // collectors - they are populated by the module-level sensors
      ProjectTypeCollector.class,
      ReportPathCollector.class,
      HashProvider.class,
      DotNetRulesDefinition.class,
      GlobalProtobufFileProcessor.class,
      RoslynRules.class,
      VbNetSonarWayProfile.class,
      // sensor
      DotNetSensor.class,
      VbNetFileCacheSensor.class,
      // language-specific
      metadata,
      VbNetCorePluginMetadata.VbNet.class,
      VbNetLanguageConfiguration.class,
      // filters
      EncodingPerFile.class,
      GeneratedFileFilter.class,
      WrongEncodingFileFilter.class,
      // importers / exporters
      ProtobufDataImporter.class,
      RoslynDataImporter.class,
      RoslynProfileExporter.class,
      SonarLintProfileExporter.class);

    context.addExtensions(new VbNetPropertyDefinitions(metadata).create());
    context.addExtensions(new CodeCoverageProvider(metadata).extensions());
    context.addExtensions(new UnitTestResultsProvider(metadata).extensions());
    context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties(metadata));
  }
}
