/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.api;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.nio.charset.Charset;
import java.util.Collection;

import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Resource;
import org.sonar.squid.Squid;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import com.sonar.csharp.CSharpAstScanner;
import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.plugins.csharp.api.tree.CSharpResourcesBridge;

public class CSharpResourcesBridgeTest {

  private static CSharpResourcesBridge cSharpResourcesBridge;
  private static Squid squid;

  @BeforeClass
  public static void init() {
    cSharpResourcesBridge = CSharpResourcesBridge.getInstance();
    squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanDirectory(new java.io.File(CSharpResourcesBridgeTest.class.getResource("/tree").getFile()));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.values());

    Collection<SourceCode> squidFiles = squid.search(new QueryByType(SourceFile.class));
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

  @Test
  public void testGetFromMember() {
    Resource<?> file = cSharpResourcesBridge.getFromMemberName("NUnit.Core.NUnitFramework#GetIgnoreReason");
    assertThat(file.getName(), is("NUnitFramework.cs"));
  }

}
