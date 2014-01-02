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
package com.sonar.csharp.squid.tree;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceClass;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.csharp.squid.parser.RuleTest;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.AstScanner;
import org.apache.commons.io.FileUtils;
import org.hamcrest.Matcher;
import org.junit.Test;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import java.io.File;
import java.nio.charset.Charset;
import java.util.Collection;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.isOneOf;
import static org.junit.Assert.assertThat;

/**
 * Test namespaces hierarchy, with classes
 * @author stevpet
 *
 */
public class CSharpTypeVisitorNamespacesTest {
  @Test
  public void testScanFile() {
    AstScanner<Grammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/namespaces/NestedNamespaces.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    Collection<SourceCode> squidClasses = scanner.getIndex().search(new QueryByType(SourceClass.class));
    Matcher<String> classesKeys = isOneOf("Namespace1.ClassBeforeNestedNamespace1", 
    		"NestedNamespace1.ClassInsideNestedNamespace1", 
    		"Namespace1.ClassAfterNestedNamespace1");
    assertThat(squidClasses.size(),is(3));
    for (SourceCode sourceCode : squidClasses) {
      assertThat(sourceCode.getKey(), classesKeys);
    }
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

  
}
