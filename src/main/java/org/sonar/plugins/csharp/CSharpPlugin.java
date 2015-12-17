/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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

import com.google.common.collect.ImmutableList;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.PropertyType;
import org.sonar.api.SonarPlugin;

import java.util.List;

@Properties({
  @Property(
    key = CSharpPlugin.FILE_SUFFIXES_KEY,
    defaultValue = CSharpPlugin.FILE_SUFFIXES_DEFVALUE,
    name = "File suffixes",
    description = "Comma-separated list of suffixes of files to analyze.",
    project = true, global = true
  ),
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
public class CSharpPlugin extends SonarPlugin {

  public static final String LANGUAGE_KEY = "cs";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FILE_SUFFIXES_KEY = "sonar.cs.file.suffixes";
  public static final String FILE_SUFFIXES_DEFVALUE = ".cs";

  public static final String CSHARP_WAY_PROFILE = "Sonar way";

  public static final String REPOSITORY_KEY = "csharpsquid";
  public static final String REPOSITORY_NAME = "SonarQube";

  public static final String IGNORE_HEADER_COMMENTS = "sonar.cs.ignoreHeaderComments";

  @Override
  public List getExtensions() {
    ImmutableList.Builder builder = ImmutableList.builder();

    builder.add(
      CSharp.class,
      CSharpSonarRulesDefinition.class,
      CSharpSonarWayProfile.class,
      CSharpCommonRulesEngine.class,
      CSharpCommonRulesDecorator.class,
      CSharpSourceCodeColorizer.class,
      RuleRunnerExtractor.class,
      CSharpSensor.class,
      CSharpCPDMapping.class,
      SonarLintProfileExporter.class,
      SonarLintParameterProfileExporter.class,
      SonarLintFakeProfileImporter.class);

    builder.addAll(CSharpFxCopProvider.extensions());
    builder.addAll(CSharpCodeCoverageProvider.extensions());
    builder.addAll(CSharpUnitTestResultsProvider.extensions());
    builder.addAll(CSharpMsBuildIntegrationProvider.extensions());

    return builder.build();
  }

}
