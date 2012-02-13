/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.unsafe;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class PrimaryNoArrayCreationExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.primaryNoArrayCreationExpression);
    g.pointerMemberAccess.mock();
    g.pointerElementAccess.mock();
    g.sizeOfExpression.mock();
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
