/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api.ast;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Token;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.squid.api.CheckMessage;
import org.sonar.squid.api.CodeCheck;
import org.sonar.squid.api.SourceFile;

/**
 * Base class to implement a C# check. A check is simply a CSharpAstVisitor with some additional methods to log messages.
 */
public abstract class CSharpAstCheck extends CSharpAstVisitor implements CodeCheck {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpAstCheck.class);

  /**
   * @param the
   *          message to log
   * @param the
   *          AST node associated to this message. Used to associate the message to a line number.
   * @param the
   *          option message's parameters (see the java.text.MessageFormat class of the java API)
   */
  protected final void log(String messageText, AstNode node, Object... messageParameters) {
    log(messageText, node.getToken(), messageParameters);
  }

  /**
   * @param the
   *          message to log
   * @param the
   *          token associated to this message. Used to associate the message to a line number.
   * @param the
   *          option message's parameters (see the java.text.MessageFormat class of the java API)
   */
  protected final void log(String messageText, Token token, Object... messageParameters) {
    CheckMessage message = new CheckMessage(this, messageText, messageParameters);
    message.setLine(token.getLine());
    log(message);
  }

  /**
   * @param the
   *          message to log
   * @param the
   *          line number to associate this to.
   * @param the
   *          option message's parameters (see the java.text.MessageFormat class of the java API)
   */
  protected final void log(String messageText, int line, Object... messageParameters) {
    CheckMessage message = new CheckMessage(this, messageText, messageParameters);
    if (line > 0) {
      message.setLine(line);
    }
    log(message);
  }

  private void log(CheckMessage message) {
    if (peekSourceCode() instanceof SourceFile) {
      peekSourceCode().log(message);
    } else if (peekSourceCode().getParent(SourceFile.class) != null) {
      peekSourceCode().getParent(SourceFile.class).log(message);
    } else {
      LOG.error("Unable to log a check message on source code '" + peekSourceCode().getKey() + "'");
    }
  }

  public final String getKey() {
    return getClass().getSimpleName();
  }

}
