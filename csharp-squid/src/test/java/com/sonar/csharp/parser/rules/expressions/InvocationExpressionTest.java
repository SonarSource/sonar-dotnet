/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class InvocationExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.invocationExpression);
  }

  @Test
  public void testOk() {
    g.primaryExpression.mock();
    g.argumentList.mock();
    assertThat(p, parse("primaryExpression()"));
    assertThat(p, parse("primaryExpression(argumentList)"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("GetAssemblies()"));
    assertThat(p, parse("dbCommand.Dispose()"));
    assertThat(p, parse("buf.Append(\"Exception during StringFormat: \").Append(formatException.Message)"));
    assertThat(p, parse("string.Format(@\"<samepath \"\"{0}\"\" {1}>\", path, defaultCaseSensitivity)"));
    assertThat(p, parse("new CollectionContainsConstraint().Using<string>( foo)"));
    assertThat(p, parse("new CollectionContainsConstraint().Using<string>( (x,y)=>String.Compare(x, y, true) )"));
    assertThat(p, parse("ReferenceProductList.Find(item => item.Id == prdId)"));
  }

}
