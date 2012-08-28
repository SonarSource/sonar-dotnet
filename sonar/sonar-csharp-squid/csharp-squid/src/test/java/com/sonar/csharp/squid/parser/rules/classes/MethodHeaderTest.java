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
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class MethodHeaderTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.methodHeader);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.returnType.mock();
    g.memberName.mock();
    g.typeParameterList.mock();
    g.formalParameterList.mock();
    g.typeParameterConstraintsClauses.mock();
    assertThat(p, parse("returnType memberName ( ) "));
    assertThat(p, parse("attributes new returnType memberName typeParameterList ( formalParameterList ) typeParameterConstraintsClauses"));
    assertThat(p, parse("public protected internal private static virtual sealed override abstract extern returnType memberName ( ) "));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("public partial void OnError()"));
    assertThat(p,
        parse("public static IEnumerable<TSource> Where<TSource>( this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)"));
    assertThat(p, parse("public bool Contains(RequestStatusDto? status, UserActionDto? action, OTCTypeDto? dealType)"));
  }

}
