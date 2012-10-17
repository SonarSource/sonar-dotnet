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
