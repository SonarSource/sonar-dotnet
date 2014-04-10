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

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import org.sonar.squidbridge.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.sslr.grammar.GrammarRuleKey;

import javax.annotation.Nullable;
import java.util.Map;

@Rule(
  key = "S1481",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class UnusedLocalVariableCheck extends SquidCheck<Grammar> {

  private static class LocalVariable {
    final AstNode declaration;
    int usages;

    private LocalVariable(AstNode declaration, int usages) {
      this.declaration = declaration;
      this.usages = usages;
    }
  }

  private static class Scope {
    private final Scope outerScope;
    private final Map<String, LocalVariable> variables;

    public Scope(Scope outerScope) {
      this.outerScope = outerScope;
      this.variables = Maps.newHashMap();
    }


    private void declare(AstNode astNode) {
      String identifier = astNode.getTokenValue();
      if (!variables.containsKey(identifier)) {
        variables.put(identifier, new LocalVariable(astNode, 0));
      }
    }

    private void use(AstNode astNode) {
      String identifier = astNode.getTokenValue();
      Scope scope = this;

      while (scope != null) {
        LocalVariable var = scope.variables.get(identifier);
        if (var != null) {
          var.usages++;
          return;
        }
        scope = scope.outerScope;
      }
    }
  }

  private static final GrammarRuleKey[] FUNCTIONS = {
    CSharpGrammar.METHOD_DECLARATION,
    CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION,
    CSharpGrammar.LAMBDA_EXPRESSION};
  private Scope currentScope;


  @Override
  public void init() {
    subscribeTo(FUNCTIONS);
    subscribeTo(
      CSharpGrammar.LOCAL_VARIABLE_DECLARATION,
      CSharpGrammar.SIMPLE_NAME);
  }


  @Override
  public void visitFile(@Nullable AstNode astNode) {
    currentScope = null;
  }

  @Override
  public void visitNode(AstNode node) {
    if (node.is(FUNCTIONS)) {
      currentScope = new Scope(currentScope);

    } else if (currentScope != null && node.is(CSharpGrammar.LOCAL_VARIABLE_DECLARATION)) {
      declareVariables(node);

    } else if (currentScope != null) {
      currentScope.use(node);
    }
  }


  @Override
  public void leaveNode(AstNode astNode) {
    if (astNode.is(FUNCTIONS)) {
      reportUnusedVariable();
      currentScope = currentScope.outerScope;
    }
  }

  private void declareVariables(AstNode variableDeclaration) {
    for (AstNode varDeclarator : variableDeclaration.getChildren(CSharpGrammar.LOCAL_VARIABLE_DECLARATOR)) {
      currentScope.declare(varDeclarator.getFirstChild(GenericTokenType.IDENTIFIER));
    }
  }

  private void reportUnusedVariable() {
    for (Map.Entry<String, LocalVariable> entry : currentScope.variables.entrySet()) {
      if (entry.getValue().usages == 0) {
        getContext().createLineViolation(this, "Remove this unused \"{0}\" local variable.", entry.getValue().declaration, entry.getKey());
      }
    }
  }

}
