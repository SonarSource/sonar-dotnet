/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class PostInvocationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.postInvocation);
  }

  @Test
  public void testOk() {
    g.argumentList.mock();
    assertThat(p, parse("()"));
    assertThat(p, parse("(argumentList)"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("()"));
    assertThat(p, parse("()"));
    assertThat(p, parse("(formatException.Message)"));
    assertThat(p, parse("(@\"<samepath \"\"{0}\"\" {1}>\", path, defaultCaseSensitivity)"));
    assertThat(p, parse("( foo)"));
    assertThat(p, parse("( (x,y)=>String.Compare(x, y, true) )"));
    assertThat(p, parse("(item => item.Id == prdId)"));
    assertThat(p, parse("()"));
  }

}
