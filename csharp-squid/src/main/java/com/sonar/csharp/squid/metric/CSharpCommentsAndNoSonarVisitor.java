/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import static com.sonar.sslr.api.GenericTokenType.*;

import java.util.HashSet;
import java.util.Set;
import java.util.StringTokenizer;

import org.sonar.squid.api.SourceFile;
import org.sonar.squid.recognizer.*;

import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.Trivia;

public class CSharpCommentsAndNoSonarVisitor extends CSharpAstVisitor implements AstAndTokenVisitor {

  private static final String NOSONAR_TAG = "NOSONAR";
  private boolean seenFirstToken;

  @Override
  public void visitFile(AstNode astNode) {
    seenFirstToken = false;
  }

  /**
   * {@inheritDoc}
   */
  public void visitToken(Token token) {
    SourceFile sourceFile = peekSourceCode() instanceof SourceFile ? (SourceFile) peekSourceCode() : peekSourceCode().getParent(
        SourceFile.class);
    CodeRecognizer codeRecognizer = new CodeRecognizer(0.94, new CSharpFootprint());

    if ( !seenFirstToken && !UNKNOWN_CHAR.equals(token.getType())) {
      seenFirstToken = true;
      return;
    }

    for (Trivia trivia : token.getTrivia()) {
      if (trivia.isComment()) {
        Token commentToken = trivia.getToken();
        String comment = cleanComment(commentToken.getValue());

        if (comment.length() == 0) {
          sourceFile.add(CSharpMetric.COMMENT_BLANK_LINES, 1);
        } else {
          StringTokenizer tokenizer = new StringTokenizer(comment, "\n");
          while (tokenizer.hasMoreElements()) {
            String commentLine = tokenizer.nextToken().trim();
            if (commentLine.length() == 0) {
              sourceFile.add(CSharpMetric.COMMENT_BLANK_LINES, 1);
            } else if (codeRecognizer.isLineOfCode(commentLine)) {
              sourceFile.add(CSharpMetric.COMMENTED_OUT_CODE_LINES, 1);
            } else if (commentLine.indexOf(NOSONAR_TAG) != -1) {
              sourceFile.addNoSonarTagLine(commentToken.getLine());
            } else {
              sourceFile.add(CSharpMetric.COMMENT_LINES, 1);
            }
          }
        }

      }
    }
  }

  protected String cleanComment(String commentString) {
    String comment = commentString.trim();
    if (comment.startsWith("/") || comment.startsWith("\\") || comment.startsWith("*")) {
      comment = cleanComment(comment.substring(1));
    }
    if (comment.endsWith("*/")) {
      comment = comment.substring(0, comment.length() - 2).trim();
    }
    return comment;
  }

  static class CSharpFootprint implements LanguageFootprint {

    private final Set<Detector> detectors = new HashSet<Detector>();

    @SuppressWarnings("all")
    public CSharpFootprint() {
      detectors.add(new EndWithDetector(0.95, '}', ';', '{'));
      detectors.add(new KeywordsDetector(0.7, "||", "&&"));
      detectors.add(new KeywordsDetector(0.3, CSharpKeyword.keywordValues()));
      detectors.add(new ContainsDetector(0.95, "++", "for(", "if(", "while(", "catch(", "switch(", "try{", "else{"));
    }

    public Set<Detector> getDetectors() {
      return detectors;
    }
  }

}
