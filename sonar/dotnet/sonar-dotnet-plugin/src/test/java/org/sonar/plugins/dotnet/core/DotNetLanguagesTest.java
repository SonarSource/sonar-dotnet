/*
 * Sonar .NET Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.dotnet.core;

import org.junit.Test;

import static org.fest.assertions.Assertions.assertThat;

public class DotNetLanguagesTest {

  @Test
  public void shouldTellIfDotNetLanguage() {
    // TRUE
    assertThat(DotNetLanguages.isDotNetLanguage("cs")).isTrue();
    assertThat(DotNetLanguages.isDotNetLanguage("vbnet")).isTrue();

    // FALSE
    assertThat(DotNetLanguages.isDotNetLanguage(null)).isFalse();
    assertThat(DotNetLanguages.isDotNetLanguage("")).isFalse();
    assertThat(DotNetLanguages.isDotNetLanguage("java")).isFalse();
    assertThat(DotNetLanguages.isDotNetLanguage("vb")).isFalse();
  }

  @Test
  public void shouldGetLanguageKeyFromFileExtension() {
    assertThat(DotNetLanguages.getLanguageKeyFromFileExtension("cs")).isEqualTo("cs");
    assertThat(DotNetLanguages.getLanguageKeyFromFileExtension("vb")).isEqualTo("vbnet");
    assertThat(DotNetLanguages.getLanguageKeyFromFileExtension("aspx")).isNull();
    assertThat(DotNetLanguages.getLanguageKeyFromFileExtension("")).isNull();
    assertThat(DotNetLanguages.getLanguageKeyFromFileExtension(null)).isNull();
  }

}
