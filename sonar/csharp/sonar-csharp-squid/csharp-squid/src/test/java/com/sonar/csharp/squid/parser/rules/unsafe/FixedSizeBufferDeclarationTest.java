/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
package com.sonar.csharp.squid.parser.rules.unsafe;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class FixedSizeBufferDeclarationTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.FIXED_SIZE_BUFFER_DECLARATION));
    p.getGrammar().rule(CSharpGrammar.TYPE).override("type");
    p.getGrammar().rule(CSharpGrammar.ATTRIBUTES).override("attributes");
    p.getGrammar().rule(CSharpGrammar.FIXED_SIZE_BUFFER_MODIFIER).override("fixedSizeBufferModifier");
    p.getGrammar().rule(CSharpGrammar.FIXED_SIZE_BUFFER_DECLARATOR).override("fixedSizeBufferDeclarator");
  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("fixed type fixedSizeBufferDeclarator;")
        .matches("fixed type fixedSizeBufferDeclarator fixedSizeBufferDeclarator fixedSizeBufferDeclarator;")
        .matches("attributes fixed type fixedSizeBufferDeclarator;")
        .matches("attributes fixedSizeBufferModifier fixed type fixedSizeBufferDeclarator;")
        .matches("fixedSizeBufferModifier fixedSizeBufferModifier fixedSizeBufferModifier fixed type fixedSizeBufferDeclarator;");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("");
  }

}
