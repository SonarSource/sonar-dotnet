/*
 * Sonar C# Plugin :: C# Squid :: Checks
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
package com.sonar.csharp.checks;

import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.Trivia;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Cardinality;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;

import java.util.regex.Pattern;

@Rule(
  key = "TodoComment",
  cardinality = Cardinality.MULTIPLE,
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class TodoCommentCheck extends SquidCheck<Grammar> implements AstAndTokenVisitor {

  private static final String DEFAULT_PATTERN = "(TODO)|(todo)|(Todo)";

  @RuleProperty(
    key = "commentPattern",
    defaultValue = DEFAULT_PATTERN)
  public String commentPattern = DEFAULT_PATTERN;

  private final Pattern regexpToDivideStringByLine = Pattern.compile("(\r?\n)|(\r)");

  private Pattern regexpCommentPattern;

  @Override
  public void init() {
    super.init();
    regexpCommentPattern = Pattern.compile(commentPattern);
  }

  public void visitToken(Token token) {
    for (Trivia trivia : token.getTrivia()) {
      if (trivia.isComment()) {
        String lines[] = regexpToDivideStringByLine.split(getContext().getCommentAnalyser().getContents(
            trivia.getToken().getOriginalValue()));

        for (int lineOffset = 0; lineOffset < lines.length; lineOffset++) {
          if (regexpCommentPattern.matcher(lines[lineOffset]).find()) {
            getContext().createLineViolation(this, "This comment matches with pattern " + commentPattern,
                trivia.getToken().getLine() + lineOffset);
            break;
          }
        }
      }
    }
  }

}
