/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.scanner;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.metric.*;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.csharp.squid.tree.CSharpMemberVisitor;
import com.sonar.csharp.squid.tree.CSharpTypeVisitor;
import com.sonar.sslr.api.CommentAnalyser;
import com.sonar.sslr.impl.Parser;
import com.sonar.sslr.squid.AstScanner;
import com.sonar.sslr.squid.SquidAstVisitor;
import com.sonar.sslr.squid.SquidAstVisitorContextImpl;
import org.sonar.squid.api.SourceProject;

public final class CSharpAstScanner {

  private CSharpAstScanner() {
  }

  public static AstScanner<CSharpGrammar> create(CSharpConfiguration conf, SquidAstVisitor<CSharpGrammar>... visitors) {

    final SquidAstVisitorContextImpl<CSharpGrammar> context = new SquidAstVisitorContextImpl<CSharpGrammar>(new SourceProject("C# Project"));
    final Parser<CSharpGrammar> parser = CSharpParser.create(conf);

    AstScanner.Builder<CSharpGrammar> builder = AstScanner.<CSharpGrammar> builder(context).setBaseParser(parser);

    /* Metrics */
    builder.withMetrics(CSharpMetric.values());

    /* Comments */
    builder.setCommentAnalyser(
        new CommentAnalyser() {

          @Override
          public boolean isBlank(String line) {
            for (int i = 0; i < line.length(); i++) {
              if (Character.isLetterOrDigit(line.charAt(i))) {
                return false;
              }
            }

            return true;
          }

          @Override
          public String getContents(String comment) {
            return comment.startsWith("//") ? comment.substring(2) : comment.substring(2, comment.length() - 2);
          }

        }
        );

    /* Files */
    builder.setFilesMetric(CSharpMetric.FILES);

    /* Metrics */
    /*
     * builder.withSquidAstVisitor(CommentsVisitor.<CSharpGrammar> builder().withCommentMetric(CSharpMetric.COMMENT_LINES)
     * .withBlankCommentMetric(CSharpMetric.COMMENT_BLANK_LINES)
     * .withNoSonar(true)
     * .withIgnoreHeaderComment(true)
     * .build());
     */
    /* Visitors */
    builder.withSquidAstVisitor(new CSharpNamespaceVisitor());
    builder.withSquidAstVisitor(new CSharpTypeVisitor());
    builder.withSquidAstVisitor(new CSharpMemberVisitor());
    builder.withSquidAstVisitor(new CSharpAccessorVisitor());
    builder.withSquidAstVisitor(new CSharpLineVisitor());
    builder.withSquidAstVisitor(new CSharpLocVisitor());
    builder.withSquidAstVisitor(new CSharpStatementVisitor());
    builder.withSquidAstVisitor(new CSharpComplexityVisitor());
    builder.withSquidAstVisitor(new CSharpPublicApiVisitor());
    builder.withSquidAstVisitor(new CSharpCommentsAndNoSonarVisitor());

    /* External visitors (typically Check ones) */
    for (SquidAstVisitor<CSharpGrammar> visitor : visitors) {
      builder.withSquidAstVisitor(visitor);
    }

    return builder.build();
  }

}
