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

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;
import org.junit.Before;
import org.junit.Test;

import java.nio.charset.Charset;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class DestructorDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.destructorDeclaration);
    g.attributes.mock();
    g.destructorBody.mock();
  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("~ id() destructorBody")
        .matches("attributes ~ id() destructorBody")
        .matches("extern unsafe ~ id() destructorBody")
        .matches("attributes unsafe ~ id() destructorBody")
        .matches("attributes extern ~ id() destructorBody")
        .matches("attributes extern unsafe ~ id() destructorBody")
        .matches("attributes unsafe extern ~ id() destructorBody");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("");
  }

}
