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

import java.util.Arrays;
import java.util.List;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;
import org.sonar.plugins.dotnet.tests.UnitTestConfiguration;
import org.sonar.plugins.dotnet.tests.UnitTestResultsAggregator;
import org.sonar.plugins.dotnet.tests.UnitTestResultsImportSensor;

public class VbNetUnitTestResultsProvider {

  private static final String CATEGORY = "VB.NET";
  private static final String SUBCATEGORY = "Unit Tests";

  private static final String VISUAL_STUDIO_TEST_RESULTS_PROPERTY_KEY = "sonar.vbnet.vstest.reportsPaths";
  private static final String NUNIT_TEST_RESULTS_PROPERTY_KEY = "sonar.vbnet.nunit.reportsPaths";
  private static final String XUNIT_TEST_RESULTS_PROPERTY_KEY = "sonar.vbnet.xunit.reportsPaths";

  private static final UnitTestConfiguration UNIT_TEST_CONF = new UnitTestConfiguration(VISUAL_STUDIO_TEST_RESULTS_PROPERTY_KEY, NUNIT_TEST_RESULTS_PROPERTY_KEY,
    XUNIT_TEST_RESULTS_PROPERTY_KEY);

  private VbNetUnitTestResultsProvider() {
  }

  public static List extensions() {
    return Arrays.asList(
      VbNetUnitTestResultsAggregator.class,
      VbNetUnitTestResultsImportSensor.class,
      PropertyDefinition.builder(VISUAL_STUDIO_TEST_RESULTS_PROPERTY_KEY)
        .name("Visual Studio Test Reports Paths")
        .description("Example: \"report.trx\", \"report1.trx,report2.trx\" or \"C:/report.trx\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(NUNIT_TEST_RESULTS_PROPERTY_KEY)
        .name("NUnit Test Reports Paths")
        .description("Example: \"TestResult.xml\", \"TestResult1.xml,TestResult2.xml\" or \"C:/TestResult.xml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build());
  }

  public static class VbNetUnitTestResultsAggregator extends UnitTestResultsAggregator {

    public VbNetUnitTestResultsAggregator(Configuration configuration) {
      super(UNIT_TEST_CONF, configuration);
    }

  }

  public static class VbNetUnitTestResultsImportSensor extends UnitTestResultsImportSensor {

    public VbNetUnitTestResultsImportSensor(VbNetUnitTestResultsAggregator unitTestResultsAggregator, ProjectDefinition projectDef) {
      super(unitTestResultsAggregator, projectDef, VbNetPlugin.LANGUAGE_KEY, VbNetPlugin.LANGUAGE_NAME);
    }

  }

}
