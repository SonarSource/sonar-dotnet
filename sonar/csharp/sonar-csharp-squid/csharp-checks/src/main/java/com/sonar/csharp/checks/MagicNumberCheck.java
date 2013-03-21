/*
 * Sonar C# Plugin :: C# Squid :: Checks
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
package com.sonar.csharp.checks;

import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

@Rule(
  key = "MagicNumber",
  priority = Priority.MAJOR)
public class MagicNumberCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(
        CSharpTokenType.INTEGER_DEC_LITERAL,
        CSharpTokenType.INTEGER_HEX_LITERAL,
        CSharpTokenType.REAL_LITERAL);
  }

  @Override
  public void visitNode(AstNode node) {
    if (!isInDeclaration(node)) {
      getContext().createLineViolation(this, "Extract this magic number into a constant or variable declaration.", node);
    }
  }

  private boolean isInDeclaration(AstNode node) {
    return node.hasAncestor(CSharpGrammar.LOCAL_VARIABLE_DECLARATOR) ||
      node.hasAncestor(CSharpGrammar.VARIABLE_DECLARATOR) ||
      node.hasAncestor(CSharpGrammar.CONSTANT_DECLARATOR);
  }

}
