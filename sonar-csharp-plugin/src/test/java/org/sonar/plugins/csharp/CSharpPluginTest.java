/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.junit.jupiter.api.Test;
import org.sonar.api.Plugin;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;

class CSharpPluginTest {

  @Test
  void getExtensions() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(7, 9), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);

    Plugin.Context context = new Plugin.Context(sonarRuntime);
    new CSharpPlugin().define(context);
    assertThat(context.getExtensions()).hasSize(63);
  }

  @Test
  void pluginProperties() {
    assertThat(CSharpPlugin.METADATA.languageKey()).isEqualTo("cs");
    assertThat(CSharpPlugin.METADATA.languageName()).isEqualTo("C#");
    assertThat(CSharpPlugin.METADATA.repositoryKey()).isEqualTo("csharpsquid");
    assertThat(CSharpPlugin.METADATA.fileSuffixesKey()).isEqualTo("sonar.cs.file.suffixes");
    assertThat(CSharpPlugin.METADATA.fileSuffixesDefaultValue()).isEqualTo(".cs,.razor");
    assertThat(CSharpPlugin.METADATA.resourcesDirectory()).isEqualTo("/org/sonar/plugins/csharp");
    assertThat(CSharpPlugin.METADATA.pluginKey()).isEqualTo("csharp");
    assertThat(CSharpPlugin.METADATA.analyzerProjectName()).isEqualTo("SonarAnalyzer.CSharp");
  }
}
