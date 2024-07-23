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
import java.util.Set;
import java.util.stream.Collectors;

import org.junit.Test;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.plugins.dotnet.tests.UnitTestResultsImportSensor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

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
      "sonar.vbnet.vstest.reportsPaths",
      "sonar.vbnet.nunit.reportsPaths");
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
      "sonar.cs.vstest.reportsPaths",
      "sonar.cs.nunit.reportsPaths");
  }

  private static Set<Object> nonProperties(List<Object> extensions) {
    return extensions.stream()
      .filter(extension -> !(extension instanceof PropertyDefinition))
      .collect(Collectors.toSet());
  }

  private static Set<String> propertyKeys(List<Object> extensions) {
    return extensions.stream()
      .filter(extension -> extension instanceof PropertyDefinition)
      .map(extension -> ((PropertyDefinition) extension).key())
      .collect(Collectors.toSet());
  }

}
