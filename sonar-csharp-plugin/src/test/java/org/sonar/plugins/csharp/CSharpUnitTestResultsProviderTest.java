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

import com.google.common.collect.ImmutableSet;
import java.util.List;
import java.util.Set;
import org.junit.Test;
import org.sonar.api.SonarQubeVersion;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.utils.Version;
import org.sonar.plugins.csharp.CSharpUnitTestResultsProvider.CSharpUnitTestResultsAggregator;
import org.sonar.plugins.csharp.CSharpUnitTestResultsProvider.CSharpUnitTestResultsImportSensor;

import static org.assertj.core.api.Assertions.assertThat;

public class CSharpUnitTestResultsProviderTest {

  @Test
  public void test() {
    assertThat(nonProperties(CSharpUnitTestResultsProvider.extensions())).containsOnly(
      CSharpUnitTestResultsAggregator.class,
      CSharpUnitTestResultsImportSensor.class);
    assertThat(propertyKeys(CSharpUnitTestResultsProvider.extensions())).containsOnly(
      "sonar.cs.vstest.reportsPaths",
      "sonar.cs.nunit.reportsPaths");
  }

  @Test
  public void createInstance_CSharpUnitTestResultsImportSensor() {
    new CSharpUnitTestResultsImportSensor(new CSharpUnitTestResultsAggregator(new MapSettings().asConfig()), ProjectDefinition.create(),
      new SonarQubeVersion(Version.create(6, 7)));
  }

  private static Set<Object> nonProperties(List extensions) {
    ImmutableSet.Builder<Object> builder = ImmutableSet.builder();
    for (Object extension : extensions) {
      if (!(extension instanceof PropertyDefinition)) {
        builder.add(extension);
      }
    }
    return builder.build();
  }

  private static Set<String> propertyKeys(List extensions) {
    ImmutableSet.Builder<String> builder = ImmutableSet.builder();
    for (Object extension : extensions) {
      if (extension instanceof PropertyDefinition) {
        PropertyDefinition property = (PropertyDefinition) extension;
        builder.add(property.key());
      }
    }
    return builder.build();
  }

}
