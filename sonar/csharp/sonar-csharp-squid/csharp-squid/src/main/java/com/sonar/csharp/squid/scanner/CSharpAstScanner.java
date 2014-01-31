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

import com.google.common.base.Charsets;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.CharsetAwareVisitor;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.metric.CSharpComplexityVisitor;
import com.sonar.csharp.squid.metric.CSharpPublicApiVisitor;
import com.sonar.csharp.squid.parser.CSharpGrammar;
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
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import java.io.File;
import java.util.Collection;

public final class CSharpAstScanner {

  private CSharpAstScanner() {
  }

  /**
   * Helper method for testing checks without having to deploy them on a Sonar instance.
   * Uses default parser configuration with UTF-8.
   */
  public static SourceFile scanSingleFile(File file, SquidAstVisitor<Grammar>... visitors) {
    CSharpConfiguration conf = new CSharpConfiguration(Charsets.UTF_8);
    return scanSingleFile(file, conf, visitors);
  }

  /**
   * Helper method for testing checks without having to deploy them on a Sonar instance.
   */
  public static SourceFile scanSingleFile(File file, CSharpConfiguration conf, SquidAstVisitor<Grammar>... visitors) {
    if (!file.isFile()) {
      throw new IllegalArgumentException("File '" + file + "' not found.");
    }
    AstScanner<Grammar> scanner = create(conf, visitors);
    scanner.scanFile(file);
    Collection<SourceCode> sources = scanner.getIndex().search(new QueryByType(SourceFile.class));
    if (sources.size() != 1) {
      throw new IllegalStateException("Only one SourceFile was expected whereas " + sources.size() + " has been returned.");
    }
    return (SourceFile) sources.iterator().next();
  }

  public static AstScanner<Grammar> create(CSharpConfiguration conf, SquidAstVisitor<Grammar>... visitors) {

    final SquidAstVisitorContextImpl<Grammar> context = new SquidAstVisitorContextImpl<Grammar>(new SourceProject("C# Project"));
    final Parser<Grammar> parser = CSharpParser.create(conf);

    AstScanner.Builder<Grammar> builder = AstScanner.<Grammar>builder(context).setBaseParser(parser);

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
    builder.withSquidAstVisitor(CommentsVisitor.<Grammar>builder()
      .withCommentMetric(CSharpMetric.COMMENT_LINES)
      .withNoSonar(true)
      .withIgnoreHeaderComment(conf.getIgnoreHeaderComments())
      .build());
    builder.withSquidAstVisitor(CounterVisitor.<Grammar>builder()
      .setMetricDef(CSharpMetric.STATEMENTS)
      .subscribeTo(
        CSharpGrammar.LABELED_STATEMENT,
        CSharpGrammar.DECLARATION_STATEMENT,
        CSharpGrammar.EXPRESSION_STATEMENT,
        CSharpGrammar.SELECTION_STATEMENT,
        CSharpGrammar.ITERATION_STATEMENT,
        CSharpGrammar.JUMP_STATEMENT,
        CSharpGrammar.TRY_STATEMENT,
        CSharpGrammar.CHECKED_STATEMENT,
        CSharpGrammar.UNCHECKED_STATEMENT,
        CSharpGrammar.LOCK_STATEMENT,
        CSharpGrammar.USING_STATEMENT,
        CSharpGrammar.YIELD_STATEMENT)
      .build());
    builder.withSquidAstVisitor(CounterVisitor.<Grammar>builder()
      .setMetricDef(CSharpMetric.ACCESSORS)
      .subscribeTo(
        CSharpGrammar.GET_ACCESSOR_DECLARATION,
        CSharpGrammar.SET_ACCESSOR_DECLARATION,
        CSharpGrammar.ADD_ACCESSOR_DECLARATION,
        CSharpGrammar.REMOVE_ACCESSOR_DECLARATION)
      .build());

    /* Visitors */
    builder.withSquidAstVisitor(new CSharpComplexityVisitor());
    builder.withSquidAstVisitor(new CSharpPublicApiVisitor());

    /* External visitors (typically Check ones) */
    for (SquidAstVisitor<Grammar> visitor : visitors) {
      if (visitor instanceof CharsetAwareVisitor) {
        ((CharsetAwareVisitor) visitor).setCharset(conf.getCharset());
      }
      builder.withSquidAstVisitor(visitor);
    }

    return builder.build();
  }

}
