/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import org.sonar.api.Plugin;
import org.sonar.api.SonarProduct;
import org.sonarsource.dotnet.shared.plugins.AnalysisWarningsSensor;
import org.sonarsource.dotnet.shared.plugins.CodeCoverageProvider;
import org.sonarsource.dotnet.shared.plugins.DotNetRulesDefinition;
import org.sonarsource.dotnet.shared.plugins.DotNetSensor;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.FileTypeSensor;
import org.sonarsource.dotnet.shared.plugins.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.GlobalProtobufFileProcessor;
import org.sonarsource.dotnet.shared.plugins.HashProvider;
import org.sonarsource.dotnet.shared.plugins.LogSensor;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.ProjectTypeCollector;
import org.sonarsource.dotnet.shared.plugins.PropertiesSensor;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynProfileExporter;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;
import org.sonarsource.dotnet.shared.plugins.SonarLintProfileExporter;
import org.sonarsource.dotnet.shared.plugins.TelemetrySensor;
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

public class CSharpCoreExtensions {

  private CSharpCoreExtensions() {
    // Private constructor to prevent instantiation
  }

  public static void register(Plugin.Context context, PluginMetadata metadata) {
    // SLCore (and OmniSharp-based SonarLint) needs only DotNetRulesDefinition and its dependencies. The rest should not be present.
    context.addExtensions(
      DotNetRulesDefinition.class,
      metadata,
      RoslynRules.class);

    if (context.getRuntime().getProduct() != SonarProduct.SONARLINT) {
      context.addExtensions(
        // module-level components (some relying on deprecated Scanner APIs)
        ModuleConfiguration.class,
        FileTypeSensor.class,
        LogSensor.class,
        TelemetrySensor.class,
        PropertiesSensor.class,
        // global components
        // collectors - they are populated by the module-level sensors
        ProjectTypeCollector.class,
        ReportPathCollector.class,
        CSharpSonarWayProfile.class,
        GlobalProtobufFileProcessor.class,
        // sensor
        DotNetSensor.class,
        // language-specific
        CSharpCorePluginMetadata.CSharp.class,
        CSharpLanguageConfiguration.class,
        // filters
        EncodingPerFile.class,
        WrongEncodingFileFilter.class,
        GeneratedFileFilter.class,
        HashProvider.class,
        // importers / exporters
        // Analysis warnings sensor is registered only here, without a language filter, to avoid pushing warnings multiple times.
        AnalysisWarningsSensor.class,
        CSharpFileCacheSensor.class,
        ProtobufDataImporter.class,
        RoslynDataImporter.class,
        RoslynProfileExporter.class,
        SonarLintProfileExporter.class);

      context.addExtensions(new CSharpPropertyDefinitions(metadata).create());
      context.addExtensions(new CodeCoverageProvider(metadata).extensions());
      context.addExtensions(new UnitTestResultsProvider(metadata).extensions());
      context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties(metadata));
    }
  }
}
