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
package com.sonar.csharp.squid.metric;

import com.google.common.collect.Sets;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.Trivia;
import com.sonar.sslr.squid.SquidAstVisitor;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.resources.File;

import java.util.List;
import java.util.Set;

/**
 * Visitor that computes the CoreMetrics.NCLOC_DATA_KEY & CoreMetrics.COMMENT_LINES_DATA_KEY metrics used by the DevCockpit.
 */
public class CSharpFileLinesVisitor extends SquidAstVisitor<Grammar> implements AstAndTokenVisitor {

  private final FileProvider fileProvider;
  private final FileLinesContextFactory fileLinesContextFactory;
  private FileLinesContext fileLinesContext;

  private final Set<Integer> linesOfCode = Sets.newHashSet();
  private final Set<Integer> linesOfComments = Sets.newHashSet();

  public CSharpFileLinesVisitor(FileProvider fileProvider, FileLinesContextFactory fileLinesContextFactory) {
    this.fileProvider = fileProvider;
    this.fileLinesContextFactory = fileLinesContextFactory;
  }

  @Override
  public void visitFile(AstNode astNode) {
    File sonarFile = fileProvider.fromIOFile(getContext().getFile());
    fileLinesContext = fileLinesContextFactory.createFor(sonarFile);
  }

  @Override
  public void leaveFile(AstNode astNode) {
    int fileLength = getContext().peekSourceCode().getInt(CSharpMetric.LINES);

    for (int line = 1; line <= fileLength; line++) {
      fileLinesContext.setIntValue(CoreMetrics.NCLOC_DATA_KEY, line, linesOfCode.contains(line) ? 1 : 0);
      fileLinesContext.setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, line, linesOfComments.contains(line) ? 1 : 0);
    }
    fileLinesContext.save();

    linesOfCode.clear();
    linesOfComments.clear();
  }

  @Override
  public void visitToken(Token token) {
    if (token.getType().equals(GenericTokenType.EOF)) {
      return;
    }

    addTokenLinesToSet(linesOfCode, token);
    List<Trivia> trivias = token.getTrivia();
    for (Trivia trivia : trivias) {
      if (trivia.isComment()) {
        addTokenLinesToSet(linesOfComments, trivia.getToken());
      }
    }
  }

  private static void addTokenLinesToSet(Set<Integer> set, Token token) {
    int currentLine = token.getLine();

    for (String line : token.getOriginalValue().split("\r\n?+|\n", -1)) {
      set.add(currentLine);
      currentLine++;
    }
  }

}
