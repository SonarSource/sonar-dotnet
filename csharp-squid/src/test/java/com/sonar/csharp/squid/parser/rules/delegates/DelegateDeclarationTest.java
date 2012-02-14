/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.delegates;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class DelegateDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.delegateDeclaration);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.returnType.mock();
    g.variantTypeParameterList.mock();
    g.formalParameterList.mock();
    g.typeParameterConstraintsClauses.mock();
    assertThat(p, parse("delegate returnType id();"));
    assertThat(p,
        parse("attributes new delegate returnType id variantTypeParameterList (formalParameterList) typeParameterConstraintsClauses;"));
    assertThat(p, parse("public protected internal private delegate returnType id();"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("delegate void CoContra2<[System.Obsolete()] out T, in K> () where T : struct;"));
  }

}
