/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.metric;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Rule;

public class CSharpStatementsVisitor extends CSharpAstVisitor {

  private Rule block = getCSharpGrammar().block;

  @Override
  public void init() {
    CSharpGrammar grammar = getCSharpGrammar();
    subscribeTo(grammar.labeledStatement, grammar.declarationStatement, grammar.embeddedStatement);
  }

  @Override
  public void visitNode(AstNode node) {
    if (block.equals(node.getFirstChild().getType())) {
      return;
    }
    peekSourceCode().add(CSharpMetric.STATEMENTS, 1);
  }
}
