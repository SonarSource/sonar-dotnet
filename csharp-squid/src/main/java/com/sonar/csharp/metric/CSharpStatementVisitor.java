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

public class CSharpStatementVisitor extends CSharpAstVisitor {

  @Override
  public void init() {
    CSharpGrammar g = getCSharpGrammar();
    subscribeTo(g.labeledStatement, g.declarationStatement, g.expressionStatement, g.selectionStatement, g.iterationStatement,
        g.jumpStatement, g.tryStatement, g.checkedStatement, g.uncheckedStatement, g.lockStatement, g.usingStatement, g.yieldStatement);
  }

  @Override
  public void visitNode(AstNode node) {
    peekSourceCode().add(CSharpMetric.STATEMENTS, 1);
  }
}
