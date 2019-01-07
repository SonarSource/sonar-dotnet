/*
 * SonarVB
 * Copyright (C) 2012-2019 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.utils.ValidationMessages;

import static org.assertj.core.api.Assertions.assertThat;

public class SonarLintFakeProfileImporterTest {

  @Test
  public void test() {
    ValidationMessages messages = ValidationMessages.create();

    SonarLintFakeProfileImporter importer = new SonarLintFakeProfileImporter();
    assertThat(importer.getSupportedLanguages()).containsOnly("vbnet");
    assertThat(importer.getName()).isEqualTo("Technical importer for the MSBuild SonarQube Scanner");

    RulesProfile profile = importer.importProfile(null, messages);
    assertThat(messages.getErrors()).containsExactly("The technical importer for the MSBuild SonarQube Scanner cannot be used.");
    assertThat(profile.getName()).isEqualTo("Technical importer for the MSBuild SonarQube Scanner");
    assertThat(profile.getLanguage()).isEqualTo("vbnet");
    assertThat(profile.getActiveRules()).isEmpty();
  }

}
