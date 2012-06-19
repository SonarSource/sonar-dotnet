/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.squid.AstScanner;
import org.apache.commons.io.FileUtils;
import org.junit.Test;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import java.io.File;
import java.nio.charset.Charset;

import static org.hamcrest.Matchers.*;
import static org.junit.Assert.*;

public class CSharpLocVisitorTest {

  @Test
  public void testScanFile() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/metric/Money.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.LINES_OF_CODE), is(278));
  }

  @Test
  public void multiLineToken() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/metric/locMultiLineToken.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.LINES_OF_CODE), is(15));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
