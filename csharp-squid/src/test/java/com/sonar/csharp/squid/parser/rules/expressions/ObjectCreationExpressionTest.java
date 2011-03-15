/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ObjectCreationExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.objectCreationExpression);
  }

  @Test
  public void testOk() {
    g.argumentList.mock();
    g.type.mock();
    g.objectOrCollectionInitializer.mock();
    assertThat(p, parse("new type()"));
    assertThat(p, parse("new type(argumentList)"));
    assertThat(p, parse("new type(argumentList) objectOrCollectionInitializer"));
    assertThat(p, parse("new type() objectOrCollectionInitializer"));
    assertThat(p, parse("new type objectOrCollectionInitializer"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("new MyClass()"));
    assertThat(p, parse("new Dictionary<int, string>  { {1, \"\"}  }"));
  }

}
