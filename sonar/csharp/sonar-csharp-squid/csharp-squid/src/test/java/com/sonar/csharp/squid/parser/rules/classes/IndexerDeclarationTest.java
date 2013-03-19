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
package com.sonar.csharp.squid.parser.rules.classes;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class IndexerDeclarationTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.INDEXER_DECLARATION));
  }

  @Test
  public void ok() {
    p.getGrammar().rule(CSharpGrammar.ATTRIBUTES).override("attributes");
    p.getGrammar().rule(CSharpGrammar.INDEXER_DECLARATOR).override("indexerDeclarator");
    p.getGrammar().rule(CSharpGrammar.ACCESSOR_DECLARATIONS).override("accessorDeclarations");

    assertThat(p)
        .matches("indexerDeclarator { accessorDeclarations }")
        .matches("attributes new indexerDeclarator { accessorDeclarations }")
        .matches("public protected internal private static virtual sealed override abstract extern indexerDeclarator { accessorDeclarations }");
  }

  @Test
  public void reallife() {
    assertThat(p)
        .matches("object IList.this[int i] { get { return (object)this[i]; } set { this[i] = (IAppender)value; } }");
  }

}
