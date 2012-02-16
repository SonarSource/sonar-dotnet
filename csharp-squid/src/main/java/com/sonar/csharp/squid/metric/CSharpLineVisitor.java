/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.squid.SquidAstVisitor;

import static com.sonar.sslr.api.GenericTokenType.*;

/**
 * Visitor that computes the number of lines of a file.
 */
public class CSharpLineVisitor extends SquidAstVisitor<CSharpGrammar> implements AstAndTokenVisitor {

  /**
   * {@inheritDoc}
   */
  public void visitToken(Token token) {
    if (token.getType() == EOF) {
      getContext().peekSourceCode().setMeasure(CSharpMetric.LINES, token.getLine());
    }
  }
}
