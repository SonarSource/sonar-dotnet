/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class TypeOfExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.typeOfExpression);
  }

  @Test
  public void testOk() {
    g.type.mock();
    g.unboundTypeName.mock();
    assertThat(p, parse("typeof (type)"));
    assertThat(p, parse("typeof (unboundTypeName)"));
    assertThat(p, parse("typeof (void)"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse("typeof (MyClass)"));
    assertThat(p, parse("typeof (CollectionProxy<>)"));
    assertThat(p, parse("typeof (List<List<int>>)"));
  }

}
