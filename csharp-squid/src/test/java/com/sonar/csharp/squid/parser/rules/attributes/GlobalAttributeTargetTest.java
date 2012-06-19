/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.attributes;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class GlobalAttributeTargetTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.globalAttributeTarget);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("assembly"));
    assertThat(p, parse("module"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
    assertThat(p, notParse("10"));
    assertThat(p, notParse("void"));
    assertThat(p, notParse("unchecked"));
  }

}
