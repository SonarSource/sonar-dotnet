/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.checks;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.squid.AstScanner;
import com.sonar.sslr.squid.SquidAstVisitor;
import org.apache.commons.io.FileUtils;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.indexer.QueryByType;

import java.io.File;
import java.util.Collection;

public class ResourceParser {

  public static SourceFile scanFile(String filePath, SquidAstVisitor<CSharpGrammar>... visitors) {
    File cFileToTest = FileUtils.toFile(ResourceParser.class.getResource(filePath));
    if (cFileToTest == null || !cFileToTest.exists()) {
      throw new AssertionError("The C# file to test '" + filePath + "' doesn't exist.");
    }

    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(), visitors);
    scanner.scanFile(cFileToTest);

    Collection<SourceCode> sources = scanner.getIndex().search(new QueryByType(SourceFile.class));
    if (sources.size() != 1) {
      throw new AssertionError("Only one SourceFile was expected whereas " + sources.size() + " has been returned.");
    } else {
      SourceFile file = (SourceFile) sources.iterator().next();
      return file;
    }
  }

}
