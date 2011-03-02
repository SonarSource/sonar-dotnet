/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.structs;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class StructBodyTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.structBody);
    g.constantDeclaration.mock();
    g.fieldDeclaration.mock();
    g.methodDeclaration.mock();
    g.propertyDeclaration.mock();
    g.eventDeclaration.mock();
    g.indexerDeclaration.mock();
    g.operatorDeclaration.mock();
    g.constructorDeclaration.mock();
    g.staticConstructorDeclaration.mock();
    g.typeDeclaration.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("{}"));
    assertThat(p, parse("{constantDeclaration}"));
    assertThat(
        p,
        parse("{fieldDeclaration methodDeclaration propertyDeclaration eventDeclaration indexerDeclaration operatorDeclaration constructorDeclaration staticConstructorDeclaration typeDeclaration}"));
  }

}
