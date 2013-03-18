/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.squid.scanner;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.metric.CSharpComplexityVisitor;
import com.sonar.csharp.squid.metric.CSharpPublicApiVisitor;
import com.sonar.csharp.squid.parser.CSharpGrammarImpl;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.csharp.squid.tree.CSharpMemberVisitor;
import com.sonar.csharp.squid.tree.CSharpTypeVisitor;
import com.sonar.sslr.api.CommentAnalyser;
import com.sonar.sslr.api.Grammar;
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

  public static AstScanner<Grammar> create(CSharpConfiguration conf, SquidAstVisitor<Grammar>... visitors) {

    final SquidAstVisitorContextImpl<Grammar> context = new SquidAstVisitorContextImpl<Grammar>(new SourceProject("C# Project"));
    final Parser<Grammar> parser = CSharpParser.create(conf);

    AstScanner.Builder<Grammar> builder = AstScanner.<Grammar> builder(context).setBaseParser(parser);

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

        });

    /* Files */
    builder.setFilesMetric(CSharpMetric.FILES);

    /* Tree */
    builder.withSquidAstVisitor(new CSharpTypeVisitor());
    builder.withSquidAstVisitor(new CSharpMemberVisitor());

    /* Metrics */
    builder.withSquidAstVisitor(new LinesVisitor<Grammar>(CSharpMetric.LINES));
    builder.withSquidAstVisitor(new LinesOfCodeVisitor<Grammar>(CSharpMetric.LINES_OF_CODE));
    builder.withSquidAstVisitor(CommentsVisitor.<Grammar> builder()
        .withCommentMetric(CSharpMetric.COMMENT_LINES)
        .withBlankCommentMetric(CSharpMetric.COMMENT_BLANK_LINES)
        .withNoSonar(true)
        .withIgnoreHeaderComment(conf.getIgnoreHeaderComments())
        .build());
    builder.withSquidAstVisitor(CounterVisitor.<Grammar> builder()
        .setMetricDef(CSharpMetric.STATEMENTS)
        .subscribeTo(
            CSharpGrammarImpl.labeledStatement,
            CSharpGrammarImpl.declarationStatement,
            CSharpGrammarImpl.expressionStatement,
            CSharpGrammarImpl.selectionStatement,
            CSharpGrammarImpl.iterationStatement,
            CSharpGrammarImpl.jumpStatement,
            CSharpGrammarImpl.tryStatement,
            CSharpGrammarImpl.checkedStatement,
            CSharpGrammarImpl.uncheckedStatement,
            CSharpGrammarImpl.lockStatement,
            CSharpGrammarImpl.usingStatement,
            CSharpGrammarImpl.yieldStatement)
        .build());
    builder.withSquidAstVisitor(CounterVisitor.<Grammar> builder()
        .setMetricDef(CSharpMetric.ACCESSORS)
        .subscribeTo(
            CSharpGrammarImpl.getAccessorDeclaration,
            CSharpGrammarImpl.setAccessorDeclaration,
            CSharpGrammarImpl.addAccessorDeclaration,
            CSharpGrammarImpl.removeAccessorDeclaration)
        .build());

    /* Visitors */
    builder.withSquidAstVisitor(new CSharpComplexityVisitor());
    builder.withSquidAstVisitor(new CSharpPublicApiVisitor());

    /* External visitors (typically Check ones) */
    for (SquidAstVisitor<Grammar> visitor : visitors) {
      builder.withSquidAstVisitor(visitor);
    }

    return builder.build();
  }

}
