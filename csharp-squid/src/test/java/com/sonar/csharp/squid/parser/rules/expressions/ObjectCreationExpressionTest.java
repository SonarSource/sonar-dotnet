/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class ObjectCreationExpressionTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.OBJECT_CREATION_EXPRESSION));
  }

  @Test
  public void ok() {
    p.getGrammar().rule(CSharpGrammar.ARGUMENT_LIST).override("argumentList");
    p.getGrammar().rule(CSharpGrammar.TYPE).override("type");
    p.getGrammar().rule(CSharpGrammar.OBJECT_OR_COLLECTION_INITIALIZER).override("objectOrCollectionInitializer");

    assertThat(p)
        .matches("new type()")
        .matches("new type(argumentList)")
        .matches("new type(argumentList) objectOrCollectionInitializer")
        .matches("new type() objectOrCollectionInitializer")
        .matches("new type objectOrCollectionInitializer");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("");
  }

  @Test
  public void reallife() {
    assertThat(p)
        .matches("new MyClass()")
        .matches("new Dictionary<int, string>  { {1, \"\"}  }");
  }

}
