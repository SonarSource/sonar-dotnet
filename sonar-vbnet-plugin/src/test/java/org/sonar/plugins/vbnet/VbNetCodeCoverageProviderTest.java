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

import com.google.common.collect.ImmutableSet;
import java.io.File;
import java.lang.reflect.Constructor;
import java.util.List;
import java.util.Set;
import org.junit.Test;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.plugins.vbnet.VbNetCodeCoverageProvider.VbNetCoverageAggregator;
import org.sonar.plugins.vbnet.VbNetCodeCoverageProvider.VbNetCoverageReportImportSensor;
import org.sonar.plugins.vbnet.VbNetCodeCoverageProvider.VbNetIntegrationCoverageAggregator;
import org.sonar.plugins.vbnet.VbNetCodeCoverageProvider.VbNetIntegrationCoverageReportImportSensor;

import static org.assertj.core.api.Assertions.assertThat;

public class VbNetCodeCoverageProviderTest {

  @Test
  public void test() {
    assertThat(nonProperties(VbNetCodeCoverageProvider.extensions())).containsOnly(
      VbNetCoverageAggregator.class,
      VbNetIntegrationCoverageAggregator.class,
      VbNetCoverageReportImportSensor.class,
      VbNetIntegrationCoverageReportImportSensor.class);
    assertThat(propertyKeys(VbNetCodeCoverageProvider.extensions())).containsOnly(
      "sonar.vbnet.ncover3.reportsPaths", "sonar.vbnet.ncover3.it.reportsPaths",
      "sonar.vbnet.opencover.reportsPaths", "sonar.vbnet.opencover.it.reportsPaths",
      "sonar.vbnet.dotcover.reportsPaths", "sonar.vbnet.dotcover.it.reportsPaths",
      "sonar.vbnet.vscoveragexml.reportsPaths", "sonar.vbnet.vscoveragexml.it.reportsPaths");
  }

  @Test
  public void for_coverage() throws Exception {
    Constructor<VbNetCodeCoverageProvider> constructor = VbNetCodeCoverageProvider.class.getDeclaredConstructor();
    assertThat(constructor.isAccessible()).isFalse();
    constructor.setAccessible(true);
    constructor.newInstance();
  }

  @Test
  public void createInstance_CoverageReport() {
    new VbNetCoverageReportImportSensor(new VbNetCoverageAggregator(new MapSettings().asConfig(), new DefaultFileSystem(new File(""))));
  }

  @Test
  public void createInstance_IntegrationCoverageReport() {
    new VbNetIntegrationCoverageReportImportSensor(new VbNetIntegrationCoverageAggregator(new MapSettings().asConfig(), new DefaultFileSystem(new File(""))));
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
