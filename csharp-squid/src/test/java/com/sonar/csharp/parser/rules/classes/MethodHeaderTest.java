/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class MethodHeaderTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

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
