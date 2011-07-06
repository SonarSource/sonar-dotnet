/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that computes the number of statements.
 */
public class CSharpStatementVisitor extends CSharpAstVisitor {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    CSharpGrammar g = getCSharpGrammar();
    subscribeTo(g.labeledStatement, g.declarationStatement, g.expressionStatement, g.selectionStatement, g.iterationStatement,
        g.jumpStatement, g.tryStatement, g.checkedStatement, g.uncheckedStatement, g.lockStatement, g.usingStatement, g.yieldStatement);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode node) {
    peekSourceCode().add(CSharpMetric.STATEMENTS, 1);
  }
}
