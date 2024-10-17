/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2024 SonarSource SA
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
package org.sonarsource.vbnet.core;

import org.sonar.api.Plugin.Context;
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
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

public class VbNetCoreExtensions {

  private VbNetCoreExtensions() {
    // Private constructor to prevent instantiation
  }

  public static void register(Context context, PluginMetadata metadata) {
    context.addExtensions(
      // module-level components (some relying on deprecated Scanner APIs)
      FileTypeSensor.class,
      LogSensor.class,
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
