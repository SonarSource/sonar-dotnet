/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.isOneOf;
import static org.junit.Assert.assertThat;

import java.io.File;
import java.nio.charset.Charset;
import java.util.Collection;

import org.apache.commons.io.FileUtils;
import org.hamcrest.Matcher;
import org.junit.Test;
import org.sonar.plugins.csharp.api.CSharpMetric;
import org.sonar.plugins.csharp.api.source.SourceMember;
import org.sonar.squid.Squid;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;

public class CSharpMemberVisitorTest {

  @Test
  public void testAllInOneMembersFile() {
    Squid squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanFile(readFile("/tree/MembersAllInOneFile.cs"));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.values());

    assertThat(project.getInt(CSharpMetric.METHODS), is(12));
    Collection<SourceCode> squidMembers = squid.search(new QueryByType(SourceMember.class));
    Matcher<String> memberKeys = isOneOf("Test#.ctor:6", "Test#.cctor():13", "Test#Finalize:20", "Test#get_Amount:28",
        "Test#get_Currency:33", "Test#set_Currency:34", "Test#get_Item:43", "Test#add_E1:53", "Test#remove_E1:54", "Test#Foo:60",
        "Test#Bar:65", "Test#op:72");
    for (SourceCode sourceCode : squidMembers) {
      assertThat(sourceCode.getKey(), memberKeys);
    }
  }

  @Test
  public void testScanFile() {
    Squid squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanFile(readFile("/tree/Money.cs"));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.values());

    // Money.cs has:
    // - 6 accessors (and 4 others that are empty and on the same line => don't count)
    // - 23 methods
    // - 2 constructors
    // = 31 members
    assertThat(project.getInt(CSharpMetric.METHODS), is(31));
  }

  @Test
  public void testScanSimpleFileWithMethodsWithSameName() {
    Squid squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanFile(readFile("/tree/simpleFile.cs"));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.values());

    assertThat(project.getInt(CSharpMetric.METHODS), is(2));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
