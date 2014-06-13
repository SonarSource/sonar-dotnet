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
import org.sonar.plugins.dotnet.tests.CoverageAggregator;
import org.sonar.plugins.dotnet.tests.CoverageConfiguration;
import org.sonar.plugins.dotnet.tests.CoverageReportImportSensor;

import java.util.List;

public class CSharpCodeCoverageProvider {

  private static final String CATEGORY = "C#";
  private static final String SUBCATEGORY = "Code Coverage";

  private static final String NCOVER3_PROPERTY_KEY = "sonar.cs.ncover3.reportsPaths";
  private static final String OPENCOVER_PROPERTY_KEY = "sonar.cs.opencover.reportsPaths";
  private static final String DOTCOVER_PROPERTY_KEY = "sonar.cs.dotcover.reportsPaths";
  private static final String VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY = "sonar.cs.vscoveragexml.reportsPaths";

  private static final CoverageConfiguration COVERAGE_CONF = new CoverageConfiguration(
    CSharpConstants.LANGUAGE_KEY,
    NCOVER3_PROPERTY_KEY,
    OPENCOVER_PROPERTY_KEY,
    DOTCOVER_PROPERTY_KEY,
    VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY);

  private CSharpCodeCoverageProvider() {
  }

  public static List extensions() {
    return ImmutableList.of(
      CSharpCoverageAggregator.class,
      CSharpCoverageReportImportSensor.class,
      PropertyDefinition.builder(NCOVER3_PROPERTY_KEY)
        .name("NCover3 Reports Paths")
        .description("Example: \"report.nccov\", \"report1.nccov,report2.nccov\" or \"C:/report.nccov\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(OPENCOVER_PROPERTY_KEY)
        .name("OpenCover Reports Paths")
        .description("Example: \"report.xml\", \"report1.xml,report2.xml\" or \"C:/report.xml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(DOTCOVER_PROPERTY_KEY)
        .name("dotCover (HTML) Reports Paths")
        .description("Example: \"report.html\", \"report1.html,report2.html\" or \"C:/report.html\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY)
        .name("Visual Studio (XML) Reports Paths")
        .description("Example: \"report.coveragexml\", \"report1.coveragexml,report2.coveragexml\" or \"C:/report.coveragexml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build());
  }

  public static class CSharpCoverageAggregator extends CoverageAggregator {

    public CSharpCoverageAggregator(Settings settings) {
      super(COVERAGE_CONF, settings);
    }

  }

  public static class CSharpCoverageReportImportSensor extends CoverageReportImportSensor {

    public CSharpCoverageReportImportSensor(CSharpCoverageAggregator coverageAggregator) {
      super(COVERAGE_CONF, coverageAggregator);
    }

  }

}
