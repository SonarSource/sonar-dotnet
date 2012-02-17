/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.scanner;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.metric.CSharpComplexityVisitor;
import com.sonar.csharp.squid.metric.CSharpPublicApiVisitor;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.csharp.squid.tree.CSharpMemberVisitor;
import com.sonar.csharp.squid.tree.CSharpTypeVisitor;
import com.sonar.sslr.api.CommentAnalyser;
import com.sonar.sslr.impl.Parser;
import com.sonar.sslr.squid.AstScanner;
import com.sonar.sslr.squid.SquidAstVisitor;
import com.sonar.sslr.squid.SquidAstVisitorContextImpl;
import com.sonar.sslr.squid.metrics.CommentsVisitor;
import com.sonar.sslr.squid.metrics.CounterVisitor;
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

    /* Tree */
    builder.withSquidAstVisitor(new CSharpTypeVisitor());
    builder.withSquidAstVisitor(new CSharpMemberVisitor());

    /* Metrics */
    builder.withSquidAstVisitor(new LinesVisitor<CSharpGrammar>(CSharpMetric.LINES));
    builder.withSquidAstVisitor(new LinesOfCodeVisitor<CSharpGrammar>(CSharpMetric.LINES_OF_CODE));
    builder.withSquidAstVisitor(CommentsVisitor.<CSharpGrammar> builder().withCommentMetric(CSharpMetric.COMMENT_LINES)
        .withBlankCommentMetric(CSharpMetric.COMMENT_BLANK_LINES)
        .withNoSonar(true)
        .withIgnoreHeaderComment(true)
        .build());
    builder.withSquidAstVisitor(CounterVisitor
        .<CSharpGrammar> builder()
        .setMetricDef(CSharpMetric.STATEMENTS)
        .subscribeTo(parser.getGrammar().labeledStatement, parser.getGrammar().declarationStatement, parser.getGrammar().expressionStatement,
            parser.getGrammar().selectionStatement, parser.getGrammar().iterationStatement, parser.getGrammar().jumpStatement, parser.getGrammar().tryStatement,
            parser.getGrammar().checkedStatement, parser.getGrammar().uncheckedStatement, parser.getGrammar().lockStatement, parser.getGrammar().usingStatement,
            parser.getGrammar().yieldStatement).build());
    builder.withSquidAstVisitor(CounterVisitor
        .<CSharpGrammar> builder()
        .setMetricDef(CSharpMetric.ACCESSORS)
        .subscribeTo(parser.getGrammar().getAccessorDeclaration, parser.getGrammar().setAccessorDeclaration, parser.getGrammar().addAccessorDeclaration,
            parser.getGrammar().removeAccessorDeclaration).build());

    /* Visitors */
    builder.withSquidAstVisitor(new CSharpComplexityVisitor());
    builder.withSquidAstVisitor(new CSharpPublicApiVisitor());

    /* External visitors (typically Check ones) */
    for (SquidAstVisitor<CSharpGrammar> visitor : visitors) {
      builder.withSquidAstVisitor(visitor);
    }

    return builder.build();
  }

}
