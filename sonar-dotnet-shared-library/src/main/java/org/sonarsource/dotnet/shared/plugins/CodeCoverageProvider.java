/*
 * SonarSource :: .NET :: Shared library
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
package org.sonarsource.dotnet.shared.plugins;

import java.util.Arrays;
import java.util.List;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.resources.Qualifiers;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.plugins.dotnet.tests.CoverageAggregator;
import org.sonar.plugins.dotnet.tests.CoverageConfiguration;
import org.sonar.plugins.dotnet.tests.CoverageReportImportSensor;
import org.sonar.plugins.dotnet.tests.ScannerFileService;

@ScannerSide
public class CodeCoverageProvider {

  private static final String SUBCATEGORY = "Code Coverage";
  private static final String SONAR_PROPERTY_PREFIX = "sonar.";

  private final DotNetPluginMetadata pluginMetadata;
  private final CoverageConfiguration coverageConf;

  public CodeCoverageProvider(DotNetPluginMetadata pluginMetadata) {
    this.pluginMetadata = pluginMetadata;
    String languageKey = pluginMetadata.languageKey();

    coverageConf = new CoverageConfiguration(
      languageKey,
      SONAR_PROPERTY_PREFIX + languageKey + ".ncover3.reportsPaths",
      SONAR_PROPERTY_PREFIX + languageKey + ".opencover.reportsPaths",
      SONAR_PROPERTY_PREFIX + languageKey + ".dotcover.reportsPaths",
      SONAR_PROPERTY_PREFIX + languageKey + ".vscoveragexml.reportsPaths");
  }

  public List<Object> extensions() {
    String category = pluginMetadata.shortLanguageName();

    return Arrays.asList(
      this,
      UnitTestCoverageAggregator.class,
      UnitTestCoverageReportImportSensor.class,

      PropertyDefinition.builder(coverageConf.ncover3PropertyKey())
        .name("NCover3 Unit Tests Reports Paths")
        .description("Example: \"report.nccov\", \"report1.nccov,report2.nccov\" or \"C:/report.nccov\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(coverageConf.openCoverPropertyKey())
        .name("OpenCover Unit Tests Reports Paths")
        .description("Example: \"report.xml\", \"report1.xml,report2.xml\" or \"C:/report.xml\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(coverageConf.dotCoverPropertyKey())
        .name("dotCover Unit Tests (HTML) Reports Paths")
        .description("Example: \"report.html\", \"report1.html,report2.html\" or \"C:/report.html\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(coverageConf.visualStudioCoverageXmlPropertyKey())
        .name("Visual Studio Unit Tests (XML) Reports Paths")
        .description("Example: \"report.coveragexml\", \"report1.coveragexml,report2.coveragexml\" or \"C:/report.coveragexml\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT)
        .multiValues(true)
        .build());
  }

  public class UnitTestCoverageAggregator extends CoverageAggregator {

    public UnitTestCoverageAggregator(Configuration configuration, FileSystem fileSystem,
      AnalysisWarnings analysisWarnings) {
      super(coverageConf,
            configuration,
            new ScannerFileService(coverageConf.languageKey(), fileSystem),
            analysisWarnings);
    }

  }

  public class UnitTestCoverageReportImportSensor extends CoverageReportImportSensor {

    public UnitTestCoverageReportImportSensor(UnitTestCoverageAggregator coverageAggregator) {
      super(coverageConf, coverageAggregator, pluginMetadata.languageKey(), pluginMetadata.languageName());
    }

  }
}
