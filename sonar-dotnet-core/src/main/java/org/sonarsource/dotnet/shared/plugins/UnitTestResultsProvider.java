/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import java.util.Arrays;
import java.util.List;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.plugins.dotnet.tests.UnitTestConfiguration;
import org.sonar.plugins.dotnet.tests.UnitTestResultsAggregator;
import org.sonar.plugins.dotnet.tests.UnitTestResultsImportSensor;

import static org.sonar.api.config.PropertyDefinition.ConfigScope;

@ScannerSide
public class UnitTestResultsProvider {

  private static final String SUBCATEGORY = "Unit Tests";

  private final PluginMetadata pluginMetadata;
  private final UnitTestConfiguration unitTestConfiguration;

  public UnitTestResultsProvider(PluginMetadata pluginMetadata) {
    this.pluginMetadata = pluginMetadata;
    this.unitTestConfiguration = new UnitTestConfiguration(propertyKey("vstest"), propertyKey("nunit"), propertyKey("xunit"));
  }

  private String propertyKey(String testType) {
    return "sonar." + pluginMetadata.languageKey() + "." + testType + ".reportsPaths";
  }

  public List<Object> extensions() {
    String category = pluginMetadata.languageName();
    return Arrays.asList(
      this,
      DotNetUnitTestResultsAggregator.class,
      UnitTestResultsImportSensor.class,
      PropertyDefinition.builder(unitTestConfiguration.visualStudioTestResultsFilePropertyKey())
        .name("Visual Studio Test Reports Paths")
        .description("Example: \"report.trx\", \"report1.trx,report2.trx\" or \"C:/report.trx\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnConfigScopes(List.of(ConfigScope.PROJECT))
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(unitTestConfiguration.nunitTestResultsFilePropertyKey())
        .name("NUnit Test Reports Paths")
        .description("Example: \"TestResult.xml\", \"TestResult1.xml,TestResult2.xml\" or \"C:/TestResult.xml\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnConfigScopes(List.of(ConfigScope.PROJECT))
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(unitTestConfiguration.xunitTestResultsFilePropertyKey())
        .name("xUnit Test Reports Paths")
        .description("Example: \"TestResult.xml\", \"TestResult1.xml,TestResult2.xml\" or \"C:/TestResult.xml\"")
        .category(category)
        .subCategory(SUBCATEGORY)
        .onlyOnConfigScopes(List.of(ConfigScope.PROJECT))
        .multiValues(true)
        .build());
  }

  public class DotNetUnitTestResultsAggregator extends UnitTestResultsAggregator {

    public DotNetUnitTestResultsAggregator(Configuration configuration) {
      super(unitTestConfiguration, configuration);
    }

  }

}
