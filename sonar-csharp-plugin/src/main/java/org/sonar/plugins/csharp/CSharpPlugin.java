/*
 * SonarC#
 * Copyright (C) 2014-2018 SonarSource SA
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
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

public class CSharpPlugin implements Plugin {

  static final String LANGUAGE_KEY = "cs";
  static final String LANGUAGE_NAME = "C#";

  static final String REPOSITORY_KEY = "csharpsquid";
  static final String REPOSITORY_NAME = "SonarAnalyzer";
  static final String PLUGIN_KEY = "csharp";
  static final String SONARANALYZER_NAME = "SonarAnalyzer.CSharp";

  static final String FILE_SUFFIXES_KEY = AbstractPropertyDefinitions.getFileSuffixProperty(LANGUAGE_KEY);
  static final String FILE_SUFFIXES_DEFVALUE = ".cs";
  static final String IGNORE_HEADER_COMMENTS = AbstractPropertyDefinitions.getIgnoreHeaderCommentsProperty(LANGUAGE_KEY);

  @Override
  public void define(Context context) {
    context.addExtensions(
      CSharp.class,
      ReportPathCollector.class,
      CSharpSonarRulesDefinition.class,
      CSharpSensor.class,
      CSharpConfiguration.class,
      WrongEncodingFileFilter.class,
      EncodingPerFile.class,
      GeneratedFileFilter.class,
      CSharpPropertiesSensor.class,
      SonarLintProfileExporter.class,
      SonarLintFakeProfileImporter.class,
      ProtobufDataImporter.class,
      RoslynDataImporter.class,
      RoslynProfileExporter.class);

    context.addExtensions(new CSharpPropertyDefinitions(context.getRuntime()).create());
    context.addExtension(new CSharpSonarWayProfile(context.getRuntime()));
    context.addExtensions(CSharpCodeCoverageProvider.extensions());
    context.addExtensions(CSharpUnitTestResultsProvider.extensions());
    context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties());
  }
}
