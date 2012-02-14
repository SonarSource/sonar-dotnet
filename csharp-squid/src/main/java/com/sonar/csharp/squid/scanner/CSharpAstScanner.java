package com.sonar.csharp.squid.scanner;

/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

import java.io.File;
import java.util.Arrays;
import java.util.Collection;
import java.util.List;
import java.util.Stack;

import org.apache.commons.io.FileUtils;
import org.apache.commons.io.filefilter.FileFilterUtils;
import org.apache.commons.io.filefilter.SuffixFileFilter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.squid.api.AnalysisException;
import org.sonar.squid.api.CodeScanner;
import org.sonar.squid.api.SourceCode;

import com.google.common.collect.Lists;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.csharp.squid.metric.*;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.csharp.squid.tree.CSharpFileVisitor;
import com.sonar.csharp.squid.tree.CSharpMemberVisitor;
import com.sonar.csharp.squid.tree.CSharpTypeVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AuditListener;
import com.sonar.sslr.api.RecognitionException;
import com.sonar.sslr.impl.Parser;
import com.sonar.sslr.impl.ast.AstWalker;

public class CSharpAstScanner extends CodeScanner<CSharpAstVisitor> {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpAstScanner.class);
  private final SourceCode project;
  private final Parser<CSharpGrammar> parser;
  private final CSharpConfiguration conf;

  public CSharpAstScanner(SourceCode project, CSharpConfiguration conf) {
    super();
    this.project = project;
    this.parser = CSharpParser.create(conf);
    this.conf = conf;
  }

  public CSharpAstScanner scanFile(File cSharpFile) {
    return scanFiles(Arrays.asList(cSharpFile));
  }

  public CSharpAstScanner scanDirectory(File directory) {
    @SuppressWarnings("unchecked")
    Collection<File> files = FileUtils.listFiles(directory, new SuffixFileFilter("cs"), FileFilterUtils.directoryFileFilter());
    return scanFiles(files);
  }

  public CSharpAstScanner scanFiles(Collection<File> files) {
    Stack<SourceCode> resourcesStack = new Stack<SourceCode>();
    resourcesStack.add(project);
    for (CSharpAstVisitor visitor : getVisitors()) {
      visitor.setProject(project);
      visitor.setSourceCodeStack(resourcesStack);
      visitor.setGrammar(parser.getGrammar());
      visitor.init();
    }
    for (File file : files) {
      try {
        parseAndVisitFile(file);
      } catch (Exception e) {
        String errorMessage = "Sonar is unable to analyze file : '" + file.getAbsolutePath() + "'";
        if ( !conf.stopSquidOnException()) {
          LOG.error(errorMessage, e);
          notifyVisitorsOfException(resourcesStack, file, e);
        } else {
          throw new AnalysisException(errorMessage, e);
        }
      }
    }
    for (CSharpAstVisitor visitor : getVisitors()) {
      visitor.destroy();
    }
    return this;
  }

  private void notifyVisitorsOfException(Stack<SourceCode> resourcesStack, File file, Exception e) {
    CSharpFileVisitor filesVisitor = new CSharpFileVisitor();
    filesVisitor.setSourceCodeStack(resourcesStack);
    filesVisitor.setFile(file);
    filesVisitor.visitFile(null);
    for (CSharpAstVisitor visitor : getVisitors()) {
      if (visitor instanceof AuditListener) {
        if (e instanceof RecognitionException) {
          ((AuditListener) visitor).processRecognitionException((RecognitionException) e);
        } else {
          ((AuditListener) visitor).processException(e);
        }
      }
    }
    filesVisitor.leaveFile(null);
  }

  private void parseAndVisitFile(File file) {
    AstNode ast = parser.parse(file);
    for (CSharpAstVisitor visitor : getVisitors()) {
      visitor.setComments(null); /* FIXME Trivias should be used! */
      visitor.setFile(file);
    }
    AstWalker astWalker = new AstWalker(getVisitors());
    astWalker.walkAndVisit(ast);
  }

  @Override
  public Collection<Class<? extends CSharpAstVisitor>> getVisitorClasses() {
    List<Class<? extends CSharpAstVisitor>> visitors = Lists.newArrayList();
    visitors.add(CSharpFileVisitor.class);
    visitors.add(CSharpNamespaceVisitor.class);
    visitors.add(CSharpTypeVisitor.class);
    visitors.add(CSharpMemberVisitor.class);
    visitors.add(CSharpAccessorVisitor.class);
    visitors.add(CSharpLineVisitor.class);
    visitors.add(CSharpLocVisitor.class);
    visitors.add(CSharpStatementVisitor.class);
    visitors.add(CSharpComplexityVisitor.class);
    visitors.add(CSharpPublicApiVisitor.class);

    visitors.add(CSharpCommentsAndNoSonarVisitor.class);
    return visitors;
  }
}
