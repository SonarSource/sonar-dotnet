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

import com.google.common.collect.ImmutableSet;
import java.util.List;
import java.util.Set;
import org.junit.Test;
import org.sonar.api.config.PropertyDefinition;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CodeCoverageProviderTest {

  @Test
  public void vbnet() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("vbnet");
    CodeCoverageProvider provider = new CodeCoverageProvider(pluginMetadata);
    assertThat(nonProperties(provider.extensions())).containsOnly(
      provider,
      CodeCoverageProvider.UnitTestCoverageAggregator.class,
      CodeCoverageProvider.IntegrationTestCoverageAggregator.class,
      CodeCoverageProvider.UnitTestCoverageReportImportSensor.class,
      CodeCoverageProvider.IntegrationTestCoverageReportImportSensor.class);
    assertThat(propertyKeys(provider.extensions())).containsOnly(
      "sonar.vbnet.ncover3.reportsPaths", "sonar.vbnet.ncover3.it.reportsPaths",
      "sonar.vbnet.opencover.reportsPaths", "sonar.vbnet.opencover.it.reportsPaths",
      "sonar.vbnet.dotcover.reportsPaths", "sonar.vbnet.dotcover.it.reportsPaths",
      "sonar.vbnet.vscoveragexml.reportsPaths", "sonar.vbnet.vscoveragexml.it.reportsPaths");
  }

  @Test
  public void csharp() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("cs");
    CodeCoverageProvider provider = new CodeCoverageProvider(pluginMetadata);
    assertThat(nonProperties(provider.extensions())).containsOnly(
      provider,
      CodeCoverageProvider.UnitTestCoverageAggregator.class,
      CodeCoverageProvider.IntegrationTestCoverageAggregator.class,
      CodeCoverageProvider.UnitTestCoverageReportImportSensor.class,
      CodeCoverageProvider.IntegrationTestCoverageReportImportSensor.class);
    assertThat(propertyKeys(provider.extensions())).containsOnly(
      "sonar.cs.ncover3.reportsPaths", "sonar.cs.ncover3.it.reportsPaths",
      "sonar.cs.opencover.reportsPaths", "sonar.cs.opencover.it.reportsPaths",
      "sonar.cs.dotcover.reportsPaths", "sonar.cs.dotcover.it.reportsPaths",
      "sonar.cs.vscoveragexml.reportsPaths", "sonar.cs.vscoveragexml.it.reportsPaths");
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
