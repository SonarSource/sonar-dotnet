/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    PluginMetadata pluginMetadata = mock(PluginMetadata.class);
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
    PluginMetadata pluginMetadata = mock(PluginMetadata.class);
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
