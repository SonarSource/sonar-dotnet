/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class SwitchStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.switchStatement);
    g.expression.mock();
    g.switchSection.mock();

  }

  @Test
  public void testOk() {
    assertThat(p, parse("switch ( expression ) {}"));
    assertThat(p, parse("switch ( expression ) { switchSection }"));
    assertThat(p, parse("switch ( expression ) { switchSection switchSection}"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("switch ( expression ) switchSection"));
    assertThat(p, notParse("switch ( expression ) { switchSection };"));
  }

}
