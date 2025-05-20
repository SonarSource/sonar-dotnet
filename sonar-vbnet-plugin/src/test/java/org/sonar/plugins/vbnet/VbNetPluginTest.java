/*
 * SonarVB
 * Copyright (C) 2012-2025 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.junit.jupiter.api.Test;
import org.sonar.api.Plugin;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;

class VbNetPluginTest {

  @Test
  void getExtensions() {
    SonarRuntime sonarRuntime = SonarRuntimeImpl.forSonarQube(Version.create(7, 9), SonarQubeSide.SCANNER, SonarEdition.COMMUNITY);

    Plugin.Context context = new Plugin.Context(sonarRuntime);
    new VbNetPlugin().define(context);
    assertThat(context.getExtensions()).hasSize(59);
  }

  @Test
  void pluginProperties() {
    assertThat(VbNetPlugin.METADATA.languageKey()).isEqualTo("vbnet");
    assertThat(VbNetPlugin.METADATA.languageName()).isEqualTo("VB.NET");
    assertThat(VbNetPlugin.METADATA.repositoryKey()).isEqualTo("vbnet");
    assertThat(VbNetPlugin.METADATA.fileSuffixesKey()).isEqualTo("sonar.vbnet.file.suffixes");
    assertThat(VbNetPlugin.METADATA.fileSuffixesDefaultValue()).isEqualTo(".vb");
    assertThat(VbNetPlugin.METADATA.resourcesDirectory()).isEqualTo("/org/sonar/plugins/vbnet");
    assertThat(VbNetPlugin.METADATA.pluginKey()).isEqualTo("vbnet");
    assertThat(VbNetPlugin.METADATA.analyzerProjectName()).isEqualTo("SonarAnalyzer.VisualBasic");
  }
}
