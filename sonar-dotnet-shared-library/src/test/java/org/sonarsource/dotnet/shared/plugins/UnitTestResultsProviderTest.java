/*
 * SonarSource :: .NET :: Shared library
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

import java.util.List;

import org.junit.Test;
import org.sonar.plugins.dotnet.tests.UnitTestResultsImportSensor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.sonarsource.dotnet.shared.PropertyUtils.nonProperties;
import static org.sonarsource.dotnet.shared.PropertyUtils.propertyKeys;

public class UnitTestResultsProviderTest {

  @Test
  public void vbnet() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("vbnet");
    UnitTestResultsProvider provider = new UnitTestResultsProvider(pluginMetadata);
    List extensions = provider.extensions();
    assertThat(nonProperties(extensions)).containsOnly(
      provider,
      UnitTestResultsProvider.DotNetUnitTestResultsAggregator.class,
      UnitTestResultsImportSensor.class);
    assertThat(propertyKeys(extensions)).containsOnly(
      "sonar.vbnet.vstest.reportsPaths", "sonar.vbnet.nunit.reportsPaths", "sonar.vbnet.xunit.reportsPaths");
  }

  @Test
  public void csharp() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("cs");
    UnitTestResultsProvider provider = new UnitTestResultsProvider(pluginMetadata);
    List extensions = provider.extensions();
    assertThat(nonProperties(extensions)).containsOnly(
      provider,
      UnitTestResultsProvider.DotNetUnitTestResultsAggregator.class,
      UnitTestResultsImportSensor.class);
    assertThat(propertyKeys(extensions)).containsOnly(
      "sonar.cs.vstest.reportsPaths", "sonar.cs.nunit.reportsPaths", "sonar.cs.xunit.reportsPaths");
  }

}
