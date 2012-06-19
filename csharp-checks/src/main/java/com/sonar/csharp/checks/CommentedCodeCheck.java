/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.checks;

import com.google.common.collect.Sets;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.Trivia;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.squid.recognizer.*;

import java.util.Set;
import java.util.regex.Pattern;

@Rule(
  key = "CommentedCode",
  priority = Priority.BLOCKER)
@BelongsToProfile(title = CSharpChecksConstants.SONAR_CSHARP_WAY_PROFILE_KEY, priority = Priority.BLOCKER)
public class CommentedCodeCheck extends SquidCheck<CSharpGrammar> implements AstAndTokenVisitor {

  private static final double THRESHOLD = 0.94;

  private final CodeRecognizer codeRecognizer = new CodeRecognizer(THRESHOLD, new CSharpRecognizer());
  private final Pattern regexpToDivideStringByLine = Pattern.compile("(\r?\n)|(\r)");

  private static class CSharpRecognizer implements LanguageFootprint {

    public Set<Detector> getDetectors() {
      Set<Detector> detectors = Sets.newHashSet();

      detectors.add(new EndWithDetector(0.95, '}', ';', '{')); // NOSONAR Magic number is suitable in this case
      detectors.add(new KeywordsDetector(0.7, "||", "&&")); // NOSONAR
      detectors.add(new KeywordsDetector(0.3, CSharpKeyword.keywordValues())); // NOSONAR
      detectors.add(new ContainsDetector(0.95, "++", "for(", "if(", "while(", "catch(", "switch(", "try{", "else{")); // NOSONAR

      return detectors;
    }

  }

  public void visitToken(Token token) {
    for (Trivia trivia : token.getTrivia()) {
      if (trivia.isComment()) {
        String lines[] = regexpToDivideStringByLine.split(getContext().getCommentAnalyser().getContents(
            trivia.getToken().getOriginalValue()));

        for (int lineOffset = 0; lineOffset < lines.length; lineOffset++) {
          if (codeRecognizer.isLineOfCode(lines[lineOffset])) {
            getContext().createLineViolation(this, "Sections of code should not be \"commented out\".",
                trivia.getToken().getLine() + lineOffset);
            break;
          }
        }
      }
    }
  }

}
