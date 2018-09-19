/*
 * SonarVB
 * Copyright (C) 2012-2018 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.sonar.api.Plugin;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

public class VbNetPlugin implements Plugin {

  static final String LANGUAGE_KEY = "vbnet";
  static final String LANGUAGE_NAME = "Visual Basic .NET";
  
  static final String REPOSITORY_KEY = "vbnet";
  static final String REPOSITORY_NAME = "SonarAnalyzer";
  static final String PLUGIN_KEY = "vbnet";
  static final String SONARANALYZER_NAME = "SonarAnalyzer.VisualBasic";

  static final String FILE_SUFFIXES_KEY = "sonar.vbnet.file.suffixes";
  static final String FILE_SUFFIXES_DEFVALUE = ".vb";
  static final String IGNORE_HEADER_COMMENTS = "sonar.vbnet.ignoreHeaderComments";

  @Override
  public void define(Context context) {
    context.addExtensions(
      VbNet.class,
      ReportPathCollector.class,
      VbNetSonarRulesDefinition.class,
      VbNetSonarWayProfile.class,
      VbNetSensor.class,
      VbNetConfiguration.class,
      WrongEncodingFileFilter.class,
      EncodingPerFile.class,
      GeneratedFileFilter.class,
      VbNetPropertiesSensor.class,
      SonarLintProfileExporter.class,
      SonarLintFakeProfileImporter.class,
      ProtobufDataImporter.class,
      RoslynDataImporter.class,
      RoslynProfileExporter.class);

    context.addExtensions(new VbNetPropertyDefinitions().create());
    context.addExtensions(VbNetCodeCoverageProvider.extensions());
    context.addExtensions(VbNetUnitTestResultsProvider.extensions());
    context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties());
  }
}
