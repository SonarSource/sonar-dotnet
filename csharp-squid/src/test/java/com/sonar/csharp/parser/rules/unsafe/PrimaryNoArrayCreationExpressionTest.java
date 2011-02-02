/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.unsafe;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class PrimaryNoArrayCreationExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.primaryNoArrayCreationExpression);
    g.unsafe.pointerMemberAccess.mock();
    g.unsafe.pointerElementAccess.mock();
    g.unsafe.sizeOfExpression.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("pointerMemberAccess"));
    // assertThat(p, parse("pointerElementAccess"));
    assertThat(p, parse("sizeOfExpression"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
