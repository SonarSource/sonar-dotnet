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

import com.google.common.base.Splitter;
import com.google.common.collect.ImmutableSet;
import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;

import java.util.Collections;
import java.util.Set;

@Rule(
  key = "MagicNumber",
  priority = Priority.MAJOR)
public class MagicNumberCheck extends SquidCheck<Grammar> {

  private static final String DEFAULT_EXCEPTIONS = "0,1,0x0,0x00";

  @RuleProperty(
    key = "exceptions",
    defaultValue = "" + DEFAULT_EXCEPTIONS)
  public String exceptions = DEFAULT_EXCEPTIONS;

  private Set<String> exceptionsSet = Collections.EMPTY_SET;

  @Override
  public void init() {
    subscribeTo(
        CSharpTokenType.INTEGER_DEC_LITERAL,
        CSharpTokenType.INTEGER_HEX_LITERAL,
        CSharpTokenType.REAL_LITERAL);

    exceptionsSet = ImmutableSet.copyOf(Splitter.on(',').omitEmptyStrings().trimResults().split(exceptions));
  }

  @Override
  public void visitNode(AstNode node) {
    if (!isInDeclaration(node) && !isExcluded(node)) {
      getContext().createLineViolation(this, "Extract this magic number into a constant or variable declaration.", node);
    }
  }

  private boolean isExcluded(AstNode node) {
    return exceptionsSet.contains(node.getTokenOriginalValue());
  }

  private boolean isInDeclaration(AstNode node) {
    return node.hasAncestor(CSharpGrammar.LOCAL_VARIABLE_DECLARATOR) ||
      node.hasAncestor(CSharpGrammar.VARIABLE_DECLARATOR) ||
      node.hasAncestor(CSharpGrammar.CONSTANT_DECLARATOR);
  }

}
