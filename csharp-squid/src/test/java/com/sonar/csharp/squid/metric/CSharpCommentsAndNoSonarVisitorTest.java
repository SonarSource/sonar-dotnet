/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import static org.hamcrest.Matchers.hasItem;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import java.io.File;
import java.nio.charset.Charset;

import org.apache.commons.io.FileUtils;
import org.junit.Test;
import org.sonar.squid.Squid;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;

import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.metric.CSharpCommentsAndNoSonarVisitor;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;

public class CSharpCommentsAndNoSonarVisitorTest {

  @Test
  public void testCleanComment() throws Exception {
    CSharpCommentsAndNoSonarVisitor commentsAndNoSonarVisitor = new CSharpCommentsAndNoSonarVisitor();
    assertThat(commentsAndNoSonarVisitor.cleanComment("/*"), is(""));
    assertThat(commentsAndNoSonarVisitor.cleanComment("//"), is(""));
    assertThat(commentsAndNoSonarVisitor.cleanComment("/////   "), is(""));
    assertThat(commentsAndNoSonarVisitor.cleanComment("/* foo"), is("foo"));
    assertThat(commentsAndNoSonarVisitor.cleanComment("// foo"), is("foo"));
    assertThat(commentsAndNoSonarVisitor.cleanComment("///// foo"), is("foo"));
    assertThat(commentsAndNoSonarVisitor.cleanComment("foo */"), is("foo"));
    assertThat(commentsAndNoSonarVisitor.cleanComment("*/"), is(""));
  }

  @Test
  public void testScanSimpleFile() {
    Squid squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanFile(readFile("/metric/simpleFile-withComments.cs"));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.values());

    assertThat(project.getInt(CSharpMetric.COMMENT_BLANK_LINES), is(2));
    assertThat(project.getInt(CSharpMetric.COMMENT_LINES), is(10));
    assertThat(project.getInt(CSharpMetric.COMMENTED_OUT_CODE_LINES), is(5));

    SourceFile file = (SourceFile) project.getFirstChild();
    assertThat(file.getNoSonarTagLines(), hasItem(17));
    assertThat(file.getNoSonarTagLines(), hasItem(48));
    assertThat(file.getNoSonarTagLines().size(), is(2));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
