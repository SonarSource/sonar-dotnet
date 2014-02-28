/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.core;

import com.google.common.collect.ImmutableList;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.config.Settings;
import org.sonar.api.resources.Qualifiers;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.dotnet.tests.CoverageConfiguration;
import org.sonar.plugins.dotnet.tests.CoverageParserFactory;
import org.sonar.plugins.dotnet.tests.CoverageReportImportSensor;

import java.util.List;

public class CSharpCodeCoverageProvider {

  private static final String CATEGORY = "C#";
  private static final String SUBCATEGORY = "Code Coverage";

  private static final String NCOVER3_PROPERTY_KEY = "sonar.csharp.ncover3.reportPath";
  private static final String OPENCOVER_PROPERTY_KEY = "sonar.csharp.opencover.reportPath";

  private static final CoverageConfiguration COVERAGE_CONF = new CoverageConfiguration(
    CSharpConstants.LANGUAGE_KEY,
    NCOVER3_PROPERTY_KEY,
    OPENCOVER_PROPERTY_KEY);

  public static List extensions() {
    return ImmutableList.of(
      CSharpCoverageParserFactory.class,
      CSharpCoverageReportImportSensor.class,
      PropertyDefinition.builder(NCOVER3_PROPERTY_KEY)
        .name("NCover3 report path")
        .description("Example: report.nccov or C:\\report.nccov")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(OPENCOVER_PROPERTY_KEY)
        .name("OpenCover report path")
        .description("Example: report.xml or C:\\report.xml")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build());
  }

  public static class CSharpCoverageParserFactory extends CoverageParserFactory {

    public CSharpCoverageParserFactory(Settings settings) {
      super(COVERAGE_CONF, settings);
    }

  }

  public static class CSharpCoverageReportImportSensor extends CoverageReportImportSensor {

    public CSharpCoverageReportImportSensor(CSharpCoverageParserFactory coverageProviderFactory) {
      super(COVERAGE_CONF, coverageProviderFactory);
    }
  }

}
