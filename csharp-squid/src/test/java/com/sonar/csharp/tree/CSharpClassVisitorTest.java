/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.isOneOf;
import static org.junit.Assert.assertThat;

import java.io.File;
import java.nio.charset.Charset;
import java.util.Collection;
import java.util.Iterator;

import org.apache.commons.io.FileUtils;
import org.hamcrest.Matcher;
import org.junit.Test;
import org.sonar.squid.Squid;
import org.sonar.squid.api.SourceClass;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import com.sonar.csharp.CSharpAstScanner;
import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.api.metric.CSharpMetric;

public class CSharpClassVisitorTest {

  @Test
  public void testScanFile() {
    Squid squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanFile(readFile("/metric/Money.cs"));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.CLASSES);

    assertThat(project.getInt(CSharpMetric.CLASSES), is(3));

    Collection<SourceCode> squidClasses = squid.search(new QueryByType(SourceClass.class));
    Matcher<String> classesKeys = isOneOf("Example.Core.Money", "Example.Core.GoodMoney", "Example.Core.GoodMoney.InnerData");
    Iterator<SourceCode> classesIterator = squidClasses.iterator();
    assertThat(classesIterator.next().getKey(), classesKeys);
    assertThat(classesIterator.next().getKey(), classesKeys);
    assertThat(classesIterator.next().getKey(), classesKeys);
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
