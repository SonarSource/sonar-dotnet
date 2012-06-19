/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.integration;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.squid.AstScanner;
import org.apache.commons.io.FileUtils;
import org.apache.commons.io.filefilter.FileFilterUtils;
import org.apache.commons.io.filefilter.SuffixFileFilter;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Resource;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.squid.Squid;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import java.nio.charset.Charset;
import java.util.Collection;

import static org.hamcrest.Matchers.*;
import static org.junit.Assert.*;
import static org.mockito.Mockito.*;

public class CSharpResourcesBridgeTest {

  private static CSharpResourcesBridge cSharpResourcesBridge;
  private static Squid squid;

  @BeforeClass
  public static void init() {
    cSharpResourcesBridge = new CSharpResourcesBridge();
    AstScanner<CSharpGrammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFiles(FileUtils.listFiles(new java.io.File(CSharpResourcesBridgeTest.class.getResource("/tree").getFile()), new SuffixFileFilter("cs"),
        FileFilterUtils.directoryFileFilter()));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    Collection<SourceCode> squidFiles = scanner.getIndex().search(new QueryByType(SourceFile.class));
    for (SourceCode squidFile : squidFiles) {
      File sonarFile = mock(File.class);
      when(sonarFile.getName()).thenReturn(squidFile.getName());
      cSharpResourcesBridge.indexFile((SourceFile) squidFile, sonarFile);
    }
  }

  @Test
  public void testGetFromType() {
    Resource<?> file = cSharpResourcesBridge.getFromTypeName("NUnit.Core", "NUnitFramework");
    assertThat(file.getName(), is("NUnitFramework.cs"));

    file = cSharpResourcesBridge.getFromTypeName("Test");
    assertThat(file.getName(), is("simpleFile.cs"));

    file = cSharpResourcesBridge.getFromTypeName("Foo.Struct.InnerClass");
    assertThat(file.getName(), is("TypesAllInOneFile.cs"));
  }

}
