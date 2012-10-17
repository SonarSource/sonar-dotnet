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
package com.sonar.csharp.squid.parser.rules.statements;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;
import org.junit.Before;
import org.junit.Test;

import java.nio.charset.Charset;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

public class LocalVariableDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.localVariableDeclaration);
  }

  @Test
  public void testOk() {
    g.type.mock();
    g.localVariableInitializer.mock();
    assertThat(p, parse("type id"));
    assertThat(p, parse("type id = localVariableInitializer"));
    assertThat(p, parse("type id1, id2 = localVariableInitializer, id3"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse("int a = 1"));
    assertThat(p, parse("int a = 1, b, c = 4"));
    assertThat(p, parse("Message message = \"Hello World\""));
    assertThat(p, parse("int num = count"));
    assertThat(p, parse("int? num = count"));
    assertThat(p, parse("Color? red = Colors.red"));
  }

}
