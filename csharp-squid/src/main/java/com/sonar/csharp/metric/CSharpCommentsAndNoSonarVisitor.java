/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.metric;

import java.util.HashSet;
import java.util.Iterator;
import java.util.Set;
import java.util.StringTokenizer;

import org.sonar.squid.api.SourceFile;
import org.sonar.squid.recognizer.CodeRecognizer;
import org.sonar.squid.recognizer.ContainsDetector;
import org.sonar.squid.recognizer.Detector;
import org.sonar.squid.recognizer.EndWithDetector;
import org.sonar.squid.recognizer.KeywordsDetector;
import org.sonar.squid.recognizer.LanguageFootprint;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Token;

public class CSharpCommentsAndNoSonarVisitor extends CSharpAstVisitor implements AstAndTokenVisitor {

  private static final String NOSONAR_TAG = "NOSONAR";
  private int firstLineOfCodeIndex = -1;

  /**
   * {@inheritDoc}
   */
  public void visitToken(Token token) {
    if (firstLineOfCodeIndex == -1) {
      if ( !token.getType().equals(GenericTokenType.UNKNOWN_CHAR)) {
        firstLineOfCodeIndex = token.getLine();
      }
    }
  }

  @Override
  public void leaveFile(AstNode astNode) {
    SourceFile sourceFile = (SourceFile) peekSourceCode();
    CodeRecognizer codeRecognizer = new CodeRecognizer(0.94, new CSharpFootprint());

    for (Iterator<Token> iterator = getComments().iterator(); iterator.hasNext();) {
      Token commentToken = (Token) iterator.next();
      // if the comment is not located before the first code token, we consider this is a header of the file and ignore it
      if (commentToken.getLine() >= firstLineOfCodeIndex) {
        String comment = cleanComment(commentToken.getValue());
        if (comment.length() == 0) {
          sourceFile.add(CSharpMetric.COMMENT_BLANK_LINES, 1);
        } else {
          StringTokenizer tokenizer = new StringTokenizer(comment, "\n");
          while (tokenizer.hasMoreElements()) {
            String commentLine = tokenizer.nextToken().trim();
            if (commentLine.isEmpty()) {
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

  class CSharpFootprint implements LanguageFootprint {

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
