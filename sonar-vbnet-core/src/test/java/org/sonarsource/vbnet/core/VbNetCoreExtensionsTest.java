/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2025 SonarSource SA
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

import org.junit.jupiter.api.Test;
import org.sonar.api.Plugin;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.utils.Version;
import org.sonarsource.dotnet.shared.plugins.CodeCoverageProvider;
import org.sonarsource.dotnet.shared.plugins.DotNetRulesDefinition;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.GlobalProtobufFileProcessor;
import org.sonarsource.dotnet.shared.plugins.HashProvider;
import org.sonarsource.dotnet.shared.plugins.MethodDeclarationsCollector;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.ProjectTypeCollector;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.RealPathProvider;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;
import org.sonarsource.dotnet.shared.plugins.TelemetryCollector;
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.filters.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.filters.WrongEncodingFileFilter;
import org.sonarsource.dotnet.shared.plugins.sensors.DotNetSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.FileTypeSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.LogSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.MethodDeclarationsSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.PropertiesSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryJsonProcessor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryJsonProjectCollector;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryJsonSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryProcessor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetrySensor;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.PropertyUtils.nonProperties;

class VbNetCoreExtensionsTest {
  @Test
  void register() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 9), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    Plugin.Context context = new Plugin.Context(sonarRuntime);
    VbNetCoreExtensions.register(context, TestVbNetMetadata.INSTANCE);
    var extensions = context.getExtensions();

    Object[] expectedExtensions = new Object[]{
      // module-level components (some relying on deprecated Scanner APIs)
      FileTypeSensor.class,
      LogSensor.class,
      MethodDeclarationsCollector.class,
      MethodDeclarationsSensor.class,
      TelemetryCollector.class,
      TelemetrySensor.class,
      TelemetryProcessor.class,
      TelemetryJsonCollector.class,
      TelemetryJsonSensor.class,
      TelemetryJsonProjectCollector.Empty.class,
      TelemetryJsonProcessor.class,
      PropertiesSensor.class,
      ModuleConfiguration.class,
      RealPathProvider.class,
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
      TestVbNetMetadata.INSTANCE,
      VbNetCorePluginMetadata.VbNet.class,
      VbNetLanguageConfiguration.class,
      // filters
      EncodingPerFile.class,
      GeneratedFileFilter.class,
      WrongEncodingFileFilter.class,
      // importers / exporters
      ProtobufDataImporter.class,
      RoslynDataImporter.class
    };

    assertThat(nonProperties(extensions)).contains(expectedExtensions);
    assertThat(extensions).hasSize(
      expectedExtensions.length
        + new CodeCoverageProvider(TestVbNetMetadata.INSTANCE).extensions().size()
        + new UnitTestResultsProvider(TestVbNetMetadata.INSTANCE).extensions().size()
        + new VbNetPropertyDefinitions(TestVbNetMetadata.INSTANCE).create().size());
  }
}
