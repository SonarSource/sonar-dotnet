/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.statements;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;
import org.junit.Before;
import org.junit.Test;

import java.nio.charset.Charset;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

public class LocalConstantDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.localConstantDeclaration);
    g.type.mock();
    g.expression.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("const type id = expression"));
    assertThat(p, parse("const type id1 = expression, id2 = expression"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("const type id"));
    assertThat(p, notParse("const type id1 = expression, id2"));
  }

}
