/*
 * SonarSource :: C# :: Core
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
package org.sonarsource.csharp.core;

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
import org.sonarsource.dotnet.shared.plugins.sensors.AnalysisWarningsSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.DotNetSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.FileTypeSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.LogSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.MethodDeclarationsSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.PropertiesSensor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryProcessor;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetrySensor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.PropertyUtils.nonProperties;

class CSharpCoreExtensionsTest {

  @Test
  void register_scanner() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(9, 9), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);
    Plugin.Context context = new Plugin.Context(sonarRuntime);
    CSharpCoreExtensions.register(context, TestCSharpMetadata.INSTANCE);
    var extensions = context.getExtensions();

    Object[] expectedExtensions = new Object[]{
      ModuleConfiguration.class,
      FileTypeSensor.class,
      LogSensor.class,
      MethodDeclarationsCollector.class,
      MethodDeclarationsSensor.class,
      TelemetryCollector.class,
      TelemetrySensor.class,
      TelemetryProcessor.class,
      RealPathProvider.class,
      PropertiesSensor.class,
      ProjectTypeCollector.class,
      ReportPathCollector.class,
      CSharpSonarWayProfile.class,
      DotNetRulesDefinition.class,
      GlobalProtobufFileProcessor.class,
      RoslynRules.class,
      DotNetSensor.class,
      TestCSharpMetadata.INSTANCE,
      CSharpCorePluginMetadata.CSharp.class,
      CSharpLanguageConfiguration.class,
      EncodingPerFile.class,
      WrongEncodingFileFilter.class,
      GeneratedFileFilter.class,
      HashProvider.class,
      AnalysisWarningsSensor.class,
      CSharpFileCacheSensor.class,
      ProtobufDataImporter.class,
      RoslynDataImporter.class
    };

    assertThat(nonProperties(extensions)).contains(expectedExtensions);

    assertThat(extensions).hasSize(
      expectedExtensions.length
        + new CodeCoverageProvider(TestCSharpMetadata.INSTANCE).extensions().size()
        + new UnitTestResultsProvider(TestCSharpMetadata.INSTANCE).extensions().size()
        + new CSharpPropertyDefinitions(TestCSharpMetadata.INSTANCE).create().size());
  }

  @Test
  void register_sonarlint() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarLint(Version.create(9, 9));
    Plugin.Context context = new Plugin.Context(sonarRuntime);
    CSharpCoreExtensions.register(context, TestCSharpMetadata.INSTANCE);
    var extensions = context.getExtensions();

    Object[] expectedExtensions = new Object[]{
      DotNetRulesDefinition.class,
      RoslynRules.class,
      TestCSharpMetadata.INSTANCE,
    };

    assertThat(extensions).containsExactlyInAnyOrder(expectedExtensions);
  }
}
