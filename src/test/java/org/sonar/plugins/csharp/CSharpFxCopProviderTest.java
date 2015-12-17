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

import com.google.common.collect.ImmutableSet;
import org.junit.Test;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.plugins.csharp.CSharpFxCopProvider.CSharpFxCopRulesDefinition;
import org.sonar.plugins.csharp.CSharpFxCopProvider.CSharpFxCopSensor;

import java.util.List;
import java.util.Set;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpFxCopProviderTest {

  @Test
  public void test() {
    assertThat(nonProperties(CSharpFxCopProvider.extensions())).containsOnly(
      CSharpFxCopRulesDefinition.class,
      CSharpFxCopSensor.class);
    assertThat(propertyKeys(CSharpFxCopProvider.extensions())).containsOnly(
      "sonar.cs.fxcop.assembly",
      "sonar.cs.fxcop.timeoutMinutes",
      "sonar.cs.fxcop.fxCopCmdPath",
      "sonar.cs.fxcop.aspnet",
      "sonar.cs.fxcop.directories",
      "sonar.cs.fxcop.references");
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
