/*
 * SonarVB
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
package org.sonar.plugins.vbnet;

import java.util.List;

import org.junit.jupiter.api.Test;
import org.sonar.api.Plugin;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.utils.Version;
import org.sonarsource.dotnet.shared.plugins.CodeCoverageProvider;
import org.sonarsource.dotnet.shared.plugins.DotNetSensor;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.plugins.FileTypeSensor;
import org.sonarsource.dotnet.shared.plugins.GeneratedFileFilter;
import org.sonarsource.dotnet.shared.plugins.HashProvider;
import org.sonarsource.dotnet.shared.plugins.LogSensor;
import org.sonarsource.dotnet.shared.plugins.ProjectTypeCollector;
import org.sonarsource.dotnet.shared.plugins.PropertiesSensor;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.ReportPathCollector;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynProfileExporter;
import org.sonarsource.dotnet.shared.plugins.SonarLintProfileExporter;
import org.sonarsource.dotnet.shared.plugins.UnitTestResultsProvider;
import org.sonarsource.dotnet.shared.plugins.WrongEncodingFileFilter;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.PropertyUtils.nonProperties;

class VbNetPluginTest {

  @Test
  void getExtensions() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(7, 9), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);

    Plugin.Context context = new Plugin.Context(sonarRuntime);
    new VbNetPlugin().define(context);

    List extensions = context.getExtensions();

    Object[] expectedExtensions = new Object[] {
      DotNetSensor.class,
      EncodingPerFile.class,
      FileTypeSensor.class,
      GeneratedFileFilter.class,
      HashProvider.class,
      LogSensor.class,
      ProjectTypeCollector.class,
      PropertiesSensor.class,
      ProtobufDataImporter.class,
      ReportPathCollector.class,
      RoslynDataImporter.class,
      RoslynProfileExporter.class,
      SonarLintProfileExporter.class,
      VbNet.class,
      VbNetFileCacheSensor.class,
      VbNetGlobalProtobufFileProcessor.class,
      VbNetLanguageConfiguration.class,
      VbNetModuleConfiguration.class,
      VbNetPlugin.METADATA,
      VbNetSonarRulesDefinition.class,
      WrongEncodingFileFilter.class
    };

    assertThat(nonProperties(extensions)).contains(expectedExtensions);

    assertThat(extensions).hasSize(
      expectedExtensions.length
        + 1 // VbNetSonarWayProfile
        + new CodeCoverageProvider(VbNetPlugin.METADATA).extensions().size()
        + new UnitTestResultsProvider(VbNetPlugin.METADATA).extensions().size()
        + RoslynProfileExporter.sonarLintRepositoryProperties(VbNetPlugin.METADATA).size()
        + new VbNetPropertyDefinitions().create().size());
  }

}
