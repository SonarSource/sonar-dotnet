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
package org.sonar.plugins.csharp;

import org.sonar.plugins.csharp.CSharpCodeCoverageProvider;
import org.sonar.plugins.csharp.CSharpCodeCoverageProvider.CSharpCoverageAggregator;
import org.sonar.plugins.csharp.CSharpCodeCoverageProvider.CSharpCoverageReportImportSensor;

import com.google.common.collect.ImmutableSet;
import org.junit.Test;
import org.sonar.api.config.PropertyDefinition;

import java.util.List;
import java.util.Set;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpCodeCoverageProviderTest {

  @Test
  public void test() {
    assertThat(nonProperties(CSharpCodeCoverageProvider.extensions())).containsOnly(
      CSharpCoverageAggregator.class,
      CSharpCoverageReportImportSensor.class);
    assertThat(propertyKeys(CSharpCodeCoverageProvider.extensions())).containsOnly(
      "sonar.cs.ncover3.reportsPaths",
      "sonar.cs.opencover.reportsPaths",
      "sonar.cs.dotcover.reportsPaths",
      "sonar.cs.vscoveragexml.reportsPaths");
  }

  private static Set<String> nonProperties(List extensions) {
    ImmutableSet.Builder builder = ImmutableSet.builder();
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
