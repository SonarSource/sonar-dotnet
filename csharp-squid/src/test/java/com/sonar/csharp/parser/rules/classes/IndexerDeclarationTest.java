/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class IndexerDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.indexerDeclaration);
    g.attributes.mock();
    g.indexerDeclarator.mock();
    g.accessorDeclarations.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("indexerDeclarator { accessorDeclarations }"));
    assertThat(p, parse("attributes new indexerDeclarator { accessorDeclarations }"));
    assertThat(
        p,
        parse("public protected internal private static virtual sealed override abstract extern indexerDeclarator { accessorDeclarations }"));
  }

}
