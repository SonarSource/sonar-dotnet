/*
 * Sonar C# Plugin :: C# Squid :: Toolkit
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
package com.sonar.csharp.toolkit;

import com.google.common.base.Charsets;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpConfigurationModelTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void getConfiguration_charset() {
    CSharpConfigurationModel model = new CSharpConfigurationModel();
    model.charsetProperty.setValue("UTF-8");
    assertThat(model.getCharset()).isEqualTo(Charsets.UTF_8);
    assertThat(model.getConfiguration().getCharset()).isEqualTo(Charsets.UTF_8);
    model.charsetProperty.setValue("ISO-8859-1");
    assertThat(model.getCharset()).isEqualTo(Charsets.ISO_8859_1);
    assertThat(model.getConfiguration().getCharset()).isEqualTo(Charsets.ISO_8859_1);
  }

  @Test
  public void getPropertyOrDefaultValue_with_property_set() {
    String oldValue = System.getProperty("foo");

    try {
      System.setProperty("foo", "bar");
      assertThat(CSharpConfigurationModel.getPropertyOrDefaultValue("foo", "baz")).isEqualTo("bar");
    } finally {
      if (oldValue == null) {
        System.clearProperty("foo");
      } else {
        System.setProperty("foo", oldValue);
      }
    }
  }

  @Test
  public void getPropertyOrDefaultValue_with_property_not_set() {
    String oldValue = System.getProperty("foo");

    try {
      System.clearProperty("foo");
      assertThat(CSharpConfigurationModel.getPropertyOrDefaultValue("foo", "baz")).isEqualTo("baz");
    } finally {
      if (oldValue == null) {
        System.clearProperty("foo");
      } else {
        System.setProperty("foo", oldValue);
      }
    }
  }

}
