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

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class BlockTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.block);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("{}"));
    assertThat(p, parse("{ int a=1; }"));
    assertThat(p, parse("{ int a=1; int b=1; int c=1; }"));
    assertThat(p, parse("{ sjkdfg ljjkdfh qsfhlmqkjdf klqjklfzaoirè!çéè'(!ru ùppo rùURZYHTFmezyrfù<aià'ra`^GJFD }"));
    assertThat(p, parse("{ // This is a comment, and the test fails...\n }"));
  }

}
