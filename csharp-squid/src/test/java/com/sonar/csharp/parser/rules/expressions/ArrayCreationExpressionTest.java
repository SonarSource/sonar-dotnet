/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class ArrayCreationExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.arrayCreationExpression);
    g.nonArrayType.mock();
    g.expressionList.mock();
    g.rankSpecifier.mock();
    g.arrayInitializer.mock();
    g.arrayType.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("new nonArrayType[expressionList]"));
    assertThat(p, parse("new nonArrayType[expressionList] rankSpecifier"));
    assertThat(p, parse("new nonArrayType[expressionList] rankSpecifier rankSpecifier"));
    assertThat(p, parse("new nonArrayType[expressionList] arrayInitializer"));
    assertThat(p, parse("new nonArrayType[expressionList] rankSpecifier rankSpecifier arrayInitializer"));
    assertThat(p, parse("new arrayType arrayInitializer"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("new arrayType"));
  }

}
