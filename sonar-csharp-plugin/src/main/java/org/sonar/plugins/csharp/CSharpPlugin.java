/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.sonar.api.Plugin;
import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
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
import org.sonarsource.dotnet.shared.plugins.SonarLintProfileExporter;
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

public class CSharpPlugin implements Plugin {

  static final String LANGUAGE_KEY = "cs";
  static final String LANGUAGE_NAME = "C#";

  static final String REPOSITORY_KEY = "csharpsquid";
  static final String PLUGIN_KEY = "csharp";
  static final String SONARANALYZER_NAME = "SonarAnalyzer.CSharp";
  static final String RESOURCES_DIRECTORY = "/org/sonar/plugins/csharp";

  static final String FILE_SUFFIXES_KEY = AbstractPropertyDefinitions.getFileSuffixProperty(LANGUAGE_KEY);
  static final String FILE_SUFFIXES_DEFVALUE = ".cs,.razor";

  static final PluginMetadata METADATA = new CSharpPluginMetadata();

  @Override
  public void define(Context context) {
    context.addExtensions(
      // module-level components (some relying on deprecated Scanner APIs)
      ModuleConfiguration.class,
      FileTypeSensor.class,
      LogSensor.class,
      PropertiesSensor.class,
      // global components
      // collectors - they are populated by the module-level sensors
      ProjectTypeCollector.class,
      ReportPathCollector.class,
      DotNetRulesDefinition.class,
      GlobalProtobufFileProcessor.class,
      // sensor
      DotNetSensor.class,
      // language-specific
      METADATA,
      CSharp.class,
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

    context.addExtensions(new CSharpPropertyDefinitions(METADATA).create());
    context.addExtension(new CSharpSonarWayProfile());
    context.addExtensions(new CodeCoverageProvider(METADATA).extensions());
    context.addExtensions(new UnitTestResultsProvider(METADATA).extensions());
    context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties(METADATA));
  }

  private static class CSharpPluginMetadata implements PluginMetadata {

    @Override
    public String languageKey() {
      return LANGUAGE_KEY;
    }

    @Override
    public String pluginKey() {
      return PLUGIN_KEY;
    }

    @Override
    public String languageName() {
      return LANGUAGE_NAME;
    }

    @Override
    public String analyzerProjectName() {
      return SONARANALYZER_NAME;
    }

    @Override
    public String repositoryKey() {
      return REPOSITORY_KEY;
    }

    @Override
    public String fileSuffixesKey() {
      return FILE_SUFFIXES_KEY;
    }

    @Override
    public String fileSuffixesDefaultValue() {
      return FILE_SUFFIXES_DEFVALUE;
    }

    @Override
    public String resourcesDirectory() {
      return RESOURCES_DIRECTORY;
    }
  }
}
