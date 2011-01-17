/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class ForStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.forStatement);
  }

  @Test
  public void testOk() {
    g.forInitializer.mock();
    g.forCondition.mock();
    g.forIterator.mock();
    g.embeddedStatement.mock();
    assertThat(p, parse("for ( ;; ) embeddedStatement"));
    assertThat(p, parse("for ( forInitializer;forCondition;forIterator ) embeddedStatement"));
    assertThat(p, parse("for ( forInitializer;;forIterator ) embeddedStatement"));
    assertThat(p, parse("for ( ;forCondition; ) embeddedStatement"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("for( int num = count;\n num > 0;\n --num)\n {}"));
//    assertThat(p, parse("for( int num = count;\n num > 0;\n --num)\n sayHello();"));
  }

}
