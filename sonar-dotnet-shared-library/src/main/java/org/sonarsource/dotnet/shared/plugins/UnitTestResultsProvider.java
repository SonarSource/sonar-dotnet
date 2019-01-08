/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;
import org.sonar.plugins.dotnet.tests.UnitTestConfiguration;
import org.sonar.plugins.dotnet.tests.UnitTestResultsAggregator;
import org.sonar.plugins.dotnet.tests.UnitTestResultsImportSensor;

@ScannerSide
public class UnitTestResultsProvider {

  private static final String SUBCATEGORY = "Unit Tests";

  private final DotNetPluginMetadata pluginMetadata;
  private final UnitTestConfiguration unitTestConfiguration;

  public UnitTestResultsProvider(DotNetPluginMetadata pluginMetadata) {
    this.pluginMetadata = pluginMetadata;
    this.unitTestConfiguration = new UnitTestConfiguration(propertyKey("vstest"), propertyKey("nunit"), propertyKey("xunit"));
  }

  private String propertyKey(String testType) {
    return "sonar." + pluginMetadata.languageKey() + "." + testType + ".reportsPaths";
  }

  public List extensions() {
    String category = pluginMetadata.shortLanguageName();
    return Arrays.asList(
      this,
      DotNetUnitTestResultsAggregator.class,
      UnitTestResultsImportSensor.class,
      PropertyDefinition.builder(unitTestConfiguration.visualStudioTestResultsFilePropertyKey())
        .name("Visual Studio Test Reports Paths")
        .description("Example: \"report.trx\", \"report1.trx,report2.trx\" or \"C:/report.trx\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(unitTestConfiguration.nunitTestResultsFilePropertyKey())
        .name("NUnit Test Reports Paths")
        .description("Example: \"TestResult.xml\", \"TestResult1.xml,TestResult2.xml\" or \"C:/TestResult.xml\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build());
  }

  public class DotNetUnitTestResultsAggregator extends UnitTestResultsAggregator {

    public DotNetUnitTestResultsAggregator(Configuration configuration) {
      super(unitTestConfiguration, configuration);
    }

  }

}
