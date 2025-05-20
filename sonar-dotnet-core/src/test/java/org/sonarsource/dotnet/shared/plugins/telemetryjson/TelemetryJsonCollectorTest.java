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
package org.sonarsource.dotnet.shared.plugins.telemetryjson;

import java.util.Map;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;

public class TelemetryJsonCollectorTest {
  @Test
  public void newTelemetryJsonCollectorIsEmpty() {
    var sut = new TelemetryJsonCollector();
    assertThat(sut.getTelemetry()).isEmpty();
  }

  @Test
  public void addedTelemetryIsRetrievable() {
    var sut = new TelemetryJsonCollector();
    sut.addTelemetry("key1", "value1");
    sut.addTelemetry(Map.entry("key2", "value2"));
    assertThat(sut.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactly(
      tuple("key1", "value1"),
      tuple("key2", "value2")
    );
  }

  @Test
  public void duplicateKeysArePreserved() {
    var sut = new TelemetryJsonCollector();
    sut.addTelemetry("key", "value1");
    sut.addTelemetry("key", "value2");
    assertThat(sut.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactly(
      tuple("key", "value1"),
      tuple("key", "value2")
    );
  }
}
