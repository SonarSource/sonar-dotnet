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
package com.sonar.csharp.squid.parser.rules.basic;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class NameSpaceOrTypeNameTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.namespaceOrTypeName);
  }

  @Test
  public void testOk() {
    g.typeArgumentList.mock();
    g.qualifiedAliasMember.mock();
    assertThat(p, parse("MyClass"));
    assertThat(p, parse("MyClass typeArgumentList"));
    assertThat(p, parse("qualifiedAliasMember"));
    assertThat(p, parse("qualifiedAliasMember.MyClass"));
    assertThat(p, parse("qualifiedAliasMember.MyClass typeArgumentList"));
    assertThat(p, parse("A.B.C.MyClass"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse("NameSpaceOrTypeNameTest"));
    assertThat(p, parse("NameSpaceOrTypeNameTest<Class>"));
    assertThat(p, parse("Foo::NonExisting"));
    assertThat(p, parse("Foo::NonExisting.Class"));
    assertThat(p, parse("Foo::NonExisting.Class<AnotherClass>"));
    assertThat(p, parse("com.sonar.csharp.squid.squid.parser.rules.basic.NameSpaceOrTypeNameTest"));
    assertThat(p, parse("com.sonar.csharp.squid.squid.parser.rules.basic.NameSpaceOrTypeNameTest<Class>"));
    assertThat(p, parse("Func<TSource, int, bool>"));
  }

}
