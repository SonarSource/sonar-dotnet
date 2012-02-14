/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class IndexerDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.indexerDeclaration);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.indexerDeclarator.mock();
    g.accessorDeclarations.mock();
    assertThat(p, parse("indexerDeclarator { accessorDeclarations }"));
    assertThat(p, parse("attributes new indexerDeclarator { accessorDeclarations }"));
    assertThat(
        p,
        parse("public protected internal private static virtual sealed override abstract extern indexerDeclarator { accessorDeclarations }"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("object IList.this[int i] { get { return (object)this[i]; } set { this[i] = (IAppender)value; } }"));
  }

}
