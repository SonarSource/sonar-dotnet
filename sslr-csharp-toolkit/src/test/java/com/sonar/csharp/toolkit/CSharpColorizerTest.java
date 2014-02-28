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

import org.junit.Test;
import org.sonar.colorizer.Tokenizer;

import java.util.List;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpColorizerTest {

  @Test
  public void getTokenizers() {
    List<Tokenizer> tokenizers = CSharpColorizer.getTokenizers();

    assertThat(tokenizers.size()).isGreaterThan(0);
  }

}
