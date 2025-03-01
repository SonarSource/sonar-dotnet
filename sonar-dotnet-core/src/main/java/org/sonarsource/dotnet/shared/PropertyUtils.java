/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared;

import org.sonar.api.config.PropertyDefinition;

import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;

public class PropertyUtils {
  private PropertyUtils() { }

  public static Set<Object> nonProperties(List<Object> extensions) {
    return extensions.stream()
      .filter(extension -> !(extension instanceof PropertyDefinition))
      .collect(Collectors.toSet());
  }

  public static Set<String> propertyKeys(List<Object> extensions) {
    return extensions.stream()
      .filter(PropertyDefinition.class::isInstance)
      .map(extension -> ((PropertyDefinition) extension).key())
      .collect(Collectors.toSet());
  }
}
