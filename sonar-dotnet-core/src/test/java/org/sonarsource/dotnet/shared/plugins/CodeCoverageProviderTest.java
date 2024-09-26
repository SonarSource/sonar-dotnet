/*
 * SonarSource :: .NET :: Core
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

import org.junit.Test;
import org.sonar.api.batch.sensor.SensorDescriptor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.sonarsource.dotnet.shared.PropertyUtils.nonProperties;
import static org.sonarsource.dotnet.shared.PropertyUtils.propertyKeys;

public class CodeCoverageProviderTest {

  @Test
  public void vbnet() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("vbnet");
    CodeCoverageProvider provider = new CodeCoverageProvider(pluginMetadata);
    assertThat(nonProperties(provider.extensions())).containsOnly(
      provider,
      CodeCoverageProvider.UnitTestCoverageAggregator.class,
      CodeCoverageProvider.UnitTestCoverageReportImportSensor.class);
    assertThat(propertyKeys(provider.extensions())).containsOnly(
      "sonar.vbnet.ncover3.reportsPaths",
      "sonar.vbnet.opencover.reportsPaths",
      "sonar.vbnet.dotcover.reportsPaths",
      "sonar.vbnet.vscoveragexml.reportsPaths");
  }

  @Test
  public void csharp() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("cs");
    CodeCoverageProvider provider = new CodeCoverageProvider(pluginMetadata);
    assertThat(nonProperties(provider.extensions())).containsOnly(
      provider,
      CodeCoverageProvider.UnitTestCoverageAggregator.class,
      CodeCoverageProvider.UnitTestCoverageReportImportSensor.class);
    assertThat(propertyKeys(provider.extensions())).containsOnly(
      "sonar.cs.ncover3.reportsPaths",
      "sonar.cs.opencover.reportsPaths",
      "sonar.cs.dotcover.reportsPaths",
      "sonar.cs.vscoveragexml.reportsPaths");
  }

  @Test
  public void verify_UnitTestCoverageReportImportSensor_constructor_uses_arguments() {
    // setup
    CodeCoverageProvider provider = createTestProvider();

    // act
    CodeCoverageProvider.UnitTestCoverageReportImportSensor sut = provider.new UnitTestCoverageReportImportSensor(
      mock(CodeCoverageProvider.UnitTestCoverageAggregator.class)
    );

    // verify that what got passed to the constructor is used later on
    SensorDescriptor mockDescriptor = mock(SensorDescriptor.class);
    sut.describe(mockDescriptor);

    verify(mockDescriptor, times(1)).name("NAME Tests Coverage Report Import");
    verify(mockDescriptor, times(1)).onlyOnLanguage("KEY");
  }

  private static CodeCoverageProvider createTestProvider() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("KEY");
    when(pluginMetadata.languageName()).thenReturn("NAME");
    return new CodeCoverageProvider(pluginMetadata);
  }
}
