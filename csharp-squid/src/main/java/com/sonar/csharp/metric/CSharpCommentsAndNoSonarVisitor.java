/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.metric;

import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.HashSet;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.measures.Metric;
import org.sonar.squid.recognizer.CodeRecognizer;
import org.sonar.squid.recognizer.ContainsDetector;
import org.sonar.squid.recognizer.Detector;
import org.sonar.squid.recognizer.EndWithDetector;
import org.sonar.squid.recognizer.KeywordsDetector;
import org.sonar.squid.recognizer.LanguageFootprint;
import org.sonar.squid.text.Source;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

// TODO Refaire cette classe en utilisant les Tokens Commentaire déjà lexés
public class CSharpCommentsAndNoSonarVisitor extends CSharpAstVisitor {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpCommentsAndNoSonarVisitor.class);

  @Override
  public void leaveFile(AstNode astNode) {
    SourceFile sourceFile = (SourceFile) peekPhysicalSourceCode();
    CodeRecognizer codeRecognizer = new CodeRecognizer(0.94, new CSharpFootprint());
    try {
      Source source = new Source(new FileReader(getFile()), codeRecognizer);
      sourceFile.add(CSharpMetric.COMMENT_BLANK_LINES, source.getMeasure(Metric.COMMENT_BLANK_LINES));
      sourceFile.add(CSharpMetric.COMMENT_LINES, source.getMeasure(Metric.COMMENT_LINES));
      sourceFile.add(CSharpMetric.COMMENTED_OUT_CODE_LINES, source.getMeasure(Metric.COMMENTED_OUT_CODE_LINES));
      sourceFile.addNoSonarTagLines(source.getNoSonarTagLines());
    } catch (FileNotFoundException e) {
      LOG.error("Unable to read C source file '" + getFile().getAbsolutePath() + "'", e);
    }
  }

  // TODO : Need to improve that, the results seem weird to me...
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
