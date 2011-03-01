/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

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

public class CSharpTreeMapTest {

  private static CSharpTreeMap cSharpTreeMap;
  private static Squid squid;

  @BeforeClass
  public static void init() {
    cSharpTreeMap = new CSharpTreeMap();
    squid = new Squid(new CSharpConfiguration(Charset.forName("UTF-8")));
    squid.register(CSharpAstScanner.class).scanDirectory(new java.io.File(CSharpTreeMapTest.class.getResource("/tree").getFile()));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.values());

    Collection<SourceCode> squidFiles = squid.search(new QueryByType(SourceFile.class));
    for (SourceCode squidFile : squidFiles) {
      File sonarFile = mock(File.class);
      when(sonarFile.getName()).thenReturn(squidFile.getName());
      cSharpTreeMap.indexFile((SourceFile) squidFile, sonarFile);
    }
  }

  @Test
  public void testGetFromType() {
    Resource<?> file = cSharpTreeMap.getFromTypeName("NUnit.Core", "NUnitFramework");
    assertThat(file.getName(), is("NUnitFramework.cs"));

    file = cSharpTreeMap.getFromTypeName("Test");
    assertThat(file.getName(), is("simpleFile.cs"));
  }

  @Test
  public void testGetFromMember() {
    Resource<?> file = cSharpTreeMap.getFromMemberName("NUnit.Core.NUnitFramework#GetIgnoreReason");
    assertThat(file.getName(), is("NUnitFramework.cs"));
  }

}
