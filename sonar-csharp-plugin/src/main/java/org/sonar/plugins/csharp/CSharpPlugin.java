/*
 * SonarC#
 * Copyright (C) 2014-2017 SonarSource SA
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
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.PropertyType;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.SonarAnalyzerScannerExtractor;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

@Properties({
  @Property(
    key = CSharpPlugin.FILE_SUFFIXES_KEY,
    defaultValue = CSharpPlugin.FILE_SUFFIXES_DEFVALUE,
    name = "File suffixes",
    description = "Comma-separated list of suffixes of files to analyze.",
    project = true, global = true),
  @Property(
    key = CSharpPlugin.IGNORE_HEADER_COMMENTS,
    defaultValue = "true",
    name = "Ignore header comments",
    description = "If set to \"true\", the file headers (that are usually the same on each file: " +
      "licensing information for example) are not considered as comments. Thus metrics such as \"Comment lines\" " +
      "do not get incremented. If set to \"false\", those file headers are considered as comments and metrics such as " +
      "\"Comment lines\" get incremented.",
    project = true, global = true,
    type = PropertyType.BOOLEAN)
})
public class CSharpPlugin implements Plugin {

  public static final String LANGUAGE_KEY = "cs";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FILE_SUFFIXES_KEY = "sonar.cs.file.suffixes";
  public static final String FILE_SUFFIXES_DEFVALUE = ".cs";
  public static final String IGNORE_HEADER_COMMENTS = "sonar.cs.ignoreHeaderComments";

  @Override
  public void define(Context context) {
    context.addExtensions(
      CSharp.class,
      CSharpSonarRulesDefinition.class,
      CSharpSonarWayProfile.class,
      SonarAnalyzerScannerExtractor.class,
      CSharpSensor.class,
      CSharpConfiguration.class,
      WrongEncodingFileFilter.class,
      EncodingPerFile.class,
      SonarLintProfileExporter.class,
      SonarLintFakeProfileImporter.class,
      RoslynProfileExporter.class);

    context.addExtensions(CSharpCodeCoverageProvider.extensions());
    context.addExtensions(CSharpUnitTestResultsProvider.extensions());
    context.addExtensions(CSharpMsBuildIntegrationProvider.extensions());
    context.addExtensions(RoslynProfileExporter.sonarLintRepositoryProperties());
  }
}
