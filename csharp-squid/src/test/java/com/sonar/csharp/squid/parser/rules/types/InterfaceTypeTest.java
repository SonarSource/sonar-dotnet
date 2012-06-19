/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class InterfaceTypeTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.interfaceType);
  }

  @Test
  public void testOk() {
    g.typeName.mock();
    assertThat(p, parse("typeName"));
  }

  @Test
  public void testKo() {
    g.typeName.mock();
    assertThat(p, notParse("object"));
    assertThat(p, notParse("string"));
    assertThat(p, notParse("this"));
    assertThat(p, notParse("typeName.this"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("ICollection"));
    assertThat(p, notParse("IList.this"));
  }

}
