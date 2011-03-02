/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class IndexerDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

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
