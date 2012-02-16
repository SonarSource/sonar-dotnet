/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.scanner;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.metric.CSharpAccessorVisitor;
import com.sonar.csharp.squid.metric.CSharpComplexityVisitor;
import com.sonar.csharp.squid.metric.CSharpPublicApiVisitor;
import com.sonar.csharp.squid.metric.CSharpStatementVisitor;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.csharp.squid.tree.CSharpMemberVisitor;
import com.sonar.csharp.squid.tree.CSharpTypeVisitor;
import com.sonar.sslr.api.CommentAnalyser;
import com.sonar.sslr.impl.Parser;
import com.sonar.sslr.squid.AstScanner;
import com.sonar.sslr.squid.SquidAstVisitor;
import com.sonar.sslr.squid.SquidAstVisitorContextImpl;
import com.sonar.sslr.squid.metrics.CommentsVisitor;
import com.sonar.sslr.squid.metrics.LinesOfCodeVisitor;
import com.sonar.sslr.squid.metrics.LinesVisitor;
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
    builder.withSquidAstVisitor(new LinesVisitor<CSharpGrammar>(CSharpMetric.LINES));
    builder.withSquidAstVisitor(new LinesOfCodeVisitor<CSharpGrammar>(CSharpMetric.LINES_OF_CODE));
    builder.withSquidAstVisitor(CommentsVisitor.<CSharpGrammar> builder().withCommentMetric(CSharpMetric.COMMENT_LINES)
        .withBlankCommentMetric(CSharpMetric.COMMENT_BLANK_LINES)
        .withNoSonar(true)
        .withIgnoreHeaderComment(true)
        .build());

    /* Visitors */
    builder.withSquidAstVisitor(new CSharpTypeVisitor());
    builder.withSquidAstVisitor(new CSharpMemberVisitor());
    builder.withSquidAstVisitor(new CSharpAccessorVisitor());
    builder.withSquidAstVisitor(new CSharpStatementVisitor());
    builder.withSquidAstVisitor(new CSharpComplexityVisitor());
    builder.withSquidAstVisitor(new CSharpPublicApiVisitor());

    /* External visitors (typically Check ones) */
    for (SquidAstVisitor<CSharpGrammar> visitor : visitors) {
      builder.withSquidAstVisitor(visitor);
    }

    return builder.build();
  }

}
