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

public class AttributeTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.attribute);
  }

  @Test
  public void testOk() {
    g.attributeName.mock();
    g.attributeArguments.mock();
    assertThat(p, parse("attributeName"));
    assertThat(p, parse("attributeName attributeArguments"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("Obsolete(\"Use Fix property\")"));
    assertThat(p, parse("Foo::NonExisting(var, 5)"));
  }

}
