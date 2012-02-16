/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceMember;
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

public class CSharpMemberVisitorTest {

  @Test
  public void testAllInOneMembersFile() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/tree/MembersAllInOneFile.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.METHODS), is(12));
    Collection<SourceCode> squidMembers = scanner.getIndex().search(new QueryByType(SourceMember.class));
    Matcher<String> memberKeys = isOneOf("Test#.ctor:6", "Test#.cctor():13", "Test#Finalize:20", "Test#get_Amount:28",
        "Test#get_Currency:33", "Test#set_Currency:34", "Test#get_Item:43", "Test#add_E1:53", "Test#remove_E1:54", "Test#Foo:60",
        "Test#Bar:65", "Test#op:72");
    for (SourceCode sourceCode : squidMembers) {
      assertThat(sourceCode.getKey(), memberKeys);
    }
  }

  @Test
  public void testScanFile() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/metric/Money.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();
    // Money.cs has:
    // - 6 accessors (and 4 others that are empty and on the same line => don't count)
    // - 23 methods
    // - 2 constructors
    // = 31 members
    assertThat(project.getInt(CSharpMetric.METHODS), is(31));
  }

  @Test
  public void testScanSimpleFileWithMethodsWithSameName() {
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/tree/simpleFile.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.METHODS), is(2));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
