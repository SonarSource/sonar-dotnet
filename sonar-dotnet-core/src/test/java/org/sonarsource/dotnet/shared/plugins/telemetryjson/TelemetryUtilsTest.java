/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

public class TelemetryUtilsTest {
  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void sanitizeKey_returnsExpectedString() {
    assertThat(TelemetryUtils.sanitizeKey("")).isEmpty();
    assertThat(TelemetryUtils.sanitizeKey("something_.12345_///hello")).isEqualTo("something__12345____hello");
    assertThat(TelemetryUtils.sanitizeKey(".NETFramework,Version=v4.7.2")).isEqualTo("_netframework_version_v4_7_2");
  }

  @Test
  public void sanitizeKey_nullInputThrows() {
    assertThatThrownBy(() -> TelemetryUtils.sanitizeKey(null))
      .isInstanceOf(NullPointerException.class)
      .hasMessageContaining("Cannot invoke \"java.lang.CharSequence.length()\" because \"this.text\" is null");
  }
}

