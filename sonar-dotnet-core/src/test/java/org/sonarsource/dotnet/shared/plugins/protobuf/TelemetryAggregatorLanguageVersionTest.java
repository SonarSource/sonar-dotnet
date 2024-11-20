/*
 * SonarSource :: .NET :: Core
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.util.Arrays;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.junit.runners.Parameterized.Parameters;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;

@RunWith(Parameterized.class)
public class TelemetryAggregatorLanguageVersionTest {
  private final String languageVersion;
  private final String expectedKey;

  @Parameters
  public static Collection<Object[]> languageVersionInputs() {
    // languageVersions(String languageVersion, String expectedKey)
    return Arrays.asList(new Object[][]
      {
        {"CS12", "plugin_key.language_key.language_version.cs12"},
        {"   ", "plugin_key.language_key.language_version.___"},
        {"With Space", "plugin_key.language_key.language_version.with_space"},
        {"Non Word Characters #+!\"§$%&/(){}", "plugin_key.language_key.language_version.non_word_characters______________"},
        {"Non Ascii Characters äöüß😉", "plugin_key.language_key.language_version.non_ascii_characters______"},
      });
  }

  public TelemetryAggregatorLanguageVersionTest(String languageVersion, String expectedKey)
  {
    this.languageVersion = languageVersion;
    this.expectedKey = expectedKey;
  }

  @Test
  public void languageVersions_sanitize() {
    var sut = new TelemetryAggregator("plugin_key", "language_key");
    var telemetries = List.of(SonarAnalyzer.Telemetry.newBuilder().setProjectFullPath("A.csproj").setLanguageVersion(languageVersion).build());
    var result = sut.aggregate(telemetries);
    assertThat(result).singleElement().extracting(Map.Entry::getKey).isEqualTo(expectedKey);
  }

  @Test
  public void languageVersions_empty() {
    var sut = new TelemetryAggregator("plugin_key", "language_key");
    var telemetries = List.of(SonarAnalyzer.Telemetry.newBuilder().setProjectFullPath("A.csproj").setLanguageVersion("").build());
    var result = sut.aggregate(telemetries);
    assertThat(result).isEmpty();
  }
}
