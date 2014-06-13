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
package org.sonar.plugins.csharp.core;

import com.google.common.collect.ImmutableList;
import org.junit.Test;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.plugins.csharp.api.CSharp;
import org.sonar.plugins.csharp.squid.CSharpRuleProfile;
import org.sonar.plugins.csharp.squid.CSharpRuleRepository;
import org.sonar.plugins.csharp.squid.CSharpSquidSensor;
import org.sonar.plugins.csharp.squid.colorizer.CSharpSourceCodeColorizer;
import org.sonar.plugins.csharp.squid.cpd.CSharpCPDMapping;

import java.util.List;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpCorePluginTest {

  @Test
  public void getExtensions() {
    assertThat(nonProperties(new CSharpCorePlugin().getExtensions())).contains(
      CSharp.class,
      CSharpSourceImporter.class,
      CSharpCommonRulesEngine.class,
      CSharpCommonRulesDecorator.class,
      CSharpCPDMapping.class,
      CSharpSourceCodeColorizer.class,
      CSharpSquidSensor.class,
      CSharpRuleRepository.class,
      CSharpRuleProfile.class);

    assertThat(new CSharpCorePlugin().getExtensions()).hasSize(
      9
        + CSharpFxCopProvider.extensions().size()
        + CSharpCodeCoverageProvider.extensions().size()
        + CSharpUnitTestResultsProvider.extensions().size());
  }

  private static List nonProperties(List extensions) {
    ImmutableList.Builder builder = ImmutableList.builder();
    for (Object extension : extensions) {
      if (!(extension instanceof PropertyDefinition)) {
        builder.add(extension);
      }
    }
    return builder.build();
  }

}
