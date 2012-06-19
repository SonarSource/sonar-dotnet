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
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import java.io.File;
import java.nio.charset.Charset;

import static org.hamcrest.Matchers.*;
import static org.junit.Assert.*;

public class CSharpCommentsAndNoSonarVisitorTest {

  @Test
  public void testWithIgnoreHeaderComments() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/metric/simpleFile-withComments.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.COMMENT_BLANK_LINES), is(12));
    assertThat(project.getInt(CSharpMetric.COMMENT_LINES), is(14));

    SourceFile file = (SourceFile) project.getFirstChild();
    assertThat(file.getNoSonarTagLines(), hasItem(24));
    assertThat(file.getNoSonarTagLines(), hasItem(55));
    assertThat(file.getNoSonarTagLines().size(), is(2));
  }

  @Test
  public void testWithoutIgnoreHeaderComments() {
    CSharpConfiguration conf = new CSharpConfiguration(Charset.forName("UTF-8"));
    conf.setIgnoreHeaderComments(false);

    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(conf);
    scanner.scanFile(readFile("/metric/simpleFile-withComments.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.COMMENT_BLANK_LINES), is(14));
    assertThat(project.getInt(CSharpMetric.COMMENT_LINES), is(17));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
