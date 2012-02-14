/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class ArrayCreationExpressionTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.arrayCreationExpression);
  }

  @Test
  public void testOk() {
    g.nonArrayType.mock();
    g.expressionList.mock();
    g.rankSpecifier.mock();
    g.arrayInitializer.mock();
    g.arrayType.mock();
    assertThat(p, parse("new nonArrayType[expressionList]"));
    assertThat(p, parse("new nonArrayType[expressionList] rankSpecifier"));
    assertThat(p, parse("new nonArrayType[expressionList] rankSpecifier rankSpecifier"));
    assertThat(p, parse("new nonArrayType[expressionList] arrayInitializer"));
    assertThat(p, parse("new nonArrayType[expressionList] rankSpecifier rankSpecifier arrayInitializer"));
    assertThat(p, parse("new arrayType arrayInitializer"));
    assertThat(p, parse("new rankSpecifier arrayInitializer"));
  }

  @Test
  public void testKo() {
    g.arrayType.mock();
    assertThat(p, notParse("new arrayType"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("new[] { 0, 1, }"));
  }

}
