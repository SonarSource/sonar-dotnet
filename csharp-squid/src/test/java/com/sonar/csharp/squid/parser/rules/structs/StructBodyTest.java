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
package com.sonar.csharp.squid.parser.rules.structs;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class StructBodyTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.STRUCT_BODY));
    p.getGrammar().rule(CSharpGrammar.CONSTANT_DECLARATION).override("constantDeclaration");
    p.getGrammar().rule(CSharpGrammar.FIELD_DECLARATION).override("fieldDeclaration");
    p.getGrammar().rule(CSharpGrammar.METHOD_DECLARATION).override("methodDeclaration");
    p.getGrammar().rule(CSharpGrammar.PROPERTY_DECLARATION).override("propertyDeclaration");
    p.getGrammar().rule(CSharpGrammar.EVENT_DECLARATION).override("eventDeclaration");
    p.getGrammar().rule(CSharpGrammar.INDEXER_DECLARATION).override("indexerDeclaration");
    p.getGrammar().rule(CSharpGrammar.OPERATOR_DECLARATION).override("operatorDeclaration");
    p.getGrammar().rule(CSharpGrammar.CONSTRUCTOR_DECLARATION).override("constructorDeclaration");
    p.getGrammar().rule(CSharpGrammar.STATIC_CONSTRUCTOR_DECLARATION).override("staticConstructorDeclaration");
    p.getGrammar().rule(CSharpGrammar.TYPE_DECLARATION).override("typeDeclaration");
  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("{}")
        .matches("{constantDeclaration}")
        .matches(
            "{fieldDeclaration methodDeclaration propertyDeclaration eventDeclaration indexerDeclaration operatorDeclaration constructorDeclaration staticConstructorDeclaration typeDeclaration}");
  }

}
