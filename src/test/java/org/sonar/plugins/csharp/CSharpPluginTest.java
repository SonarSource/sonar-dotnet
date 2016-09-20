/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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

import com.google.common.collect.ImmutableList;
import java.util.List;
import org.junit.Test;
import org.sonar.api.Plugin;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class CSharpPluginTest {

  @Test
  public void getExtensions() {
    Plugin.Context context = new Plugin.Context(mock(Version.class));
    new CSharpPlugin().define(context);

    List extensions = context.getExtensions();

    Class<?>[] expectedExtensions = new Class<?>[] {
      CSharp.class,
      CSharpCommonRulesEngine.class,
      CSharpCommonRulesDecorator.class,
      CSharpSourceCodeColorizer.class,
      CSharpSonarRulesDefinition.class,
      CSharpSonarWayProfile.class,
      RuleRunnerExtractor.class,
      CSharpSensor.class,
      CSharpCPDMapping.class,
      SonarLintProfileExporter.class,
      SonarLintFakeProfileImporter.class,
      RoslynProfileExporter.class
    };

    assertThat(nonProperties(extensions)).contains(expectedExtensions);

    assertThat(extensions).hasSize(
      expectedExtensions.length
        + CSharpFxCopProvider.extensions().size()
        + CSharpCodeCoverageProvider.extensions().size()
        + CSharpUnitTestResultsProvider.extensions().size()
        + CSharpMsBuildIntegrationProvider.extensions().size()
        + RoslynProfileExporter.sonarLintRepositoryProperties().size());
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
