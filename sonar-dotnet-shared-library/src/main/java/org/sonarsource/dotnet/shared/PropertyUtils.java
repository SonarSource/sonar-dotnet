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
