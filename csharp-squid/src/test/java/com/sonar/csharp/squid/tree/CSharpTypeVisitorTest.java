/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
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

import static org.hamcrest.Matchers.*;
import static org.junit.Assert.*;

public class CSharpTypeVisitorTest {

  @Test
  public void testScanFile() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/tree/TypesAllInOneFile.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.CLASSES), is(2));
    assertThat(project.getInt(CSharpMetric.INTERFACES), is(1));
    assertThat(project.getInt(CSharpMetric.DELEGATES), is(1));
    assertThat(project.getInt(CSharpMetric.STRUCTS), is(2));
    assertThat(project.getInt(CSharpMetric.ENUMS), is(1));

    Collection<SourceCode> squidClasses = scanner.getIndex().search(new QueryByType(SourceType.class));
    Matcher<String> classesKeys = isOneOf("Foo.Class", "Foo.Class.InnerStruct", "Foo.Struct", "Foo.Struct.InnerClass", "Foo.Enum",
        "Bar.Interface", "Bar.Delegate");
    for (SourceCode sourceCode : squidClasses) {
      assertThat(sourceCode.getKey(), classesKeys);
    }
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
