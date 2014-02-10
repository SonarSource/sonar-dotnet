/*
 * Sonar .NET Plugin :: Tests
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
package org.sonar.plugins.csharp.tests;

import com.google.common.collect.ImmutableList;
import org.sonar.api.SonarPlugin;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;

import java.util.List;

public class TestsPlugin extends SonarPlugin {

  public static final String NCOVER3_REPORT_PATH_PROPERTY = "sonar.dotnet.tests.ncover3.reportPath";
  public static final String OPEN_COVER_REPORT_PATH_PROPERTY = "sonar.dotnet.tests.opencover.reportPath";

  @Override
  public List getExtensions() {
    return ImmutableList.builder()
      .addAll(getPropertyDefinitions())
      .add(CoverageProviderFactory.class)
      .add(CoverageReportImportSensor.class)
      .build();
  }

  public static List<PropertyDefinition> getPropertyDefinitions() {
    String dotNet = ".NET";
    String subCategory = "Code coverage";
    return ImmutableList.of(
      PropertyDefinition.builder(NCOVER3_REPORT_PATH_PROPERTY)
        .category(dotNet)
        .subCategory(subCategory)
        .name("NCover3 report path")
        .description("Path (absolute or relative) to the NCover3 code coverage report.")
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(OPEN_COVER_REPORT_PATH_PROPERTY)
        .category(dotNet)
        .subCategory(subCategory)
        .name("OpenCover report path")
        .description("Path (absolute or relative) to the OpenCover code coverage report.")
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build());
  }

}
