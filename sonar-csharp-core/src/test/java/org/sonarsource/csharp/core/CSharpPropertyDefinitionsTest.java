/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.csharp.core;

import java.util.List;
import org.junit.jupiter.api.Test;
import org.sonar.api.config.PropertyDefinition;

import static org.assertj.core.api.Assertions.assertThat;

class CSharpPropertyDefinitionsTest {

  @Test
  void create() {
    CSharpPropertyDefinitions sut = new CSharpPropertyDefinitions(TestCSharpMetadata.INSTANCE);
    List<PropertyDefinition> properties = sut.create();
    assertThat(properties)
      .hasSize(13)
      .extracting(PropertyDefinition::name).containsOnlyOnce("Analyze Razor code");
  }

  @Test
  void create_containsScannerForDotNetProperties() {
    CSharpPropertyDefinitions sut = new CSharpPropertyDefinitions(TestCSharpMetadata.INSTANCE);
    List<PropertyDefinition> properties = sut.create();
    // These must exist for S4NET to download the ZIP with analyzers from the server.
    assertThat(properties)
      .extracting(PropertyDefinition::key)
      .contains(
        "sonar.cs.analyzer.dotnet.pluginKey",
        "sonar.cs.analyzer.dotnet.pluginVersion",
        "sonar.cs.analyzer.dotnet.staticResourceName");
  }

  @Test
  void getAnalyzeRazorCode() {
    CSharpPropertyDefinitions sut = new CSharpPropertyDefinitions(TestCSharpMetadata.INSTANCE);
    assertThat(sut.getAnalyzeRazorCode("LANG")).isEqualTo("sonar.LANG.analyzeRazorCode");
  }
}
