/*
 * Sonar C# Plugin :: Rules
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
package org.sonar.plugins.csharp.checks;

import java.io.File;
import java.util.Collection;
import java.util.Set;

import org.apache.commons.io.FileUtils;
import org.sonar.squid.api.CheckMessage;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceCodeTreeDecorator;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;
import org.sonar.squid.indexer.SquidIndex;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;

public class CheckUtils {

  public static Set<CheckMessage> extractViolations(String cFilePath, CSharpAstVisitor... visitors) {
    SourceProject project = new SourceProject("C# Project");
    SquidIndex index = new SquidIndex();
    index.index(project);
    CSharpAstScanner scanner = new CSharpAstScanner(project, new CSharpConfiguration());

    registerDefaultVisitors(scanner);
    registerVisitors(scanner, visitors);

    File cFileToTest = FileUtils.toFile(CheckUtils.class.getResource(cFilePath));
    if (cFileToTest == null || !cFileToTest.exists()) {
      throw new AssertionError("The C# file to test '" + cFilePath + "' doesn't exist.");
    }
    scanner.scanFile(cFileToTest);

    SourceCodeTreeDecorator decorator = new SourceCodeTreeDecorator(project);
    decorator.decorateWith(CSharpMetric.values());
    Collection<SourceCode> sources = index.search(new QueryByType(SourceFile.class));
    if (sources.size() != 1) {
      throw new AssertionError("Only one SourceFile was expected whereas " + sources.size() + " has been returned.");
    } else {
      SourceFile file = (SourceFile) sources.iterator().next();
      return file.getCheckMessages();
    }
  }

  private static void registerVisitors(CSharpAstScanner scanner, CSharpAstVisitor... visitors) {
    for (CSharpAstVisitor visitor : visitors) {
      scanner.accept(visitor);
    }
  }

  private static void registerDefaultVisitors(CSharpAstScanner scanner) {
    Collection<Class<? extends CSharpAstVisitor>> visitors = scanner.getVisitorClasses();
    for (Class<? extends CSharpAstVisitor> visitor : visitors) {
      try {
        scanner.accept(visitor.newInstance());
      } catch (Exception e) {
        throw new RuntimeException("Unable to instanciate CSharpAstVisitor : " + visitor.getCanonicalName(), e);
      }
    }
  }

  public static CheckMessage extractViolation(String cFilePath, CSharpAstVisitor... visitors) {
    Set<CheckMessage> violations = extractViolations(cFilePath, visitors);
    if (violations.size() != 1) {
      StringBuilder logMessage = new StringBuilder();
      logMessage.append("Only one violation was expected whereas " + violations.size() + " have been generated.");
      for (CheckMessage violation : violations) {
        logMessage.append("\n" + "Line " + violation.getLine() + " : " + violation.formatDefaultMessage());
      }
      throw new AssertionError(logMessage.toString());
    } else {
      return violations.iterator().next();
    }
  }
}
