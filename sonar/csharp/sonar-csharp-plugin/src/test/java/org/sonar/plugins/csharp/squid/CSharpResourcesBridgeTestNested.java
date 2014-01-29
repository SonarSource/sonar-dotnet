/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.squid;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.AstScanner;
import org.apache.commons.io.FileUtils;
import org.apache.commons.io.filefilter.FileFilterUtils;
import org.apache.commons.io.filefilter.SuffixFileFilter;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Resource;
import org.sonar.plugins.csharp.squid.CSharpResourcesBridge;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;

import java.nio.charset.Charset;
import java.util.Collection;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CSharpResourcesBridgeTestNested {

  private static CSharpResourcesBridge cSharpResourcesBridge;

  @BeforeClass
  public static void init() {
    cSharpResourcesBridge = new CSharpResourcesBridge();
    AstScanner<Grammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFiles(FileUtils.listFiles(new java.io.File(CSharpResourcesBridgeTest.class.getResource("/namespaces").getFile()), new SuffixFileFilter("cs"),
        FileFilterUtils.directoryFileFilter()));
    scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    Collection<SourceCode> squidFiles = scanner.getIndex().search(new QueryByType(SourceFile.class));
    for (SourceCode squidFile : squidFiles) {
      File sonarFile = mock(File.class);
      when(sonarFile.getName()).thenReturn(squidFile.getName());
      cSharpResourcesBridge.indexFile((SourceFile) squidFile, sonarFile);
    }
  }


  @Test
  public void testGetFromNestedNamespace() {
    assertInNestedNamespacesSource("NestedNamespace1.ClassInsideNestedNamespace1");
  }

  @Test
  public void testGetFromOutsideNestedNamespace() {
    assertInNestedNamespacesSource("Namespace1.ClassBeforeNestedNamespace1");
  }
  
  @Test
  public void testAfterNestedNamespace() {
    assertInNestedNamespacesSource("Namespace1.ClassAfterNestedNamespace1");
  }
  
  @Test
  public void testNotNestedNamespace() {
    Resource<?> file;

    file = cSharpResourcesBridge.getFromTypeName("Namespace1.ClassInNamespace1");
    assertThat(file.getName(), is("NotNestedNamespace.cs"));
  }
  
  private void assertInNestedNamespacesSource(String typeFullName) {
	  Resource<?> file = cSharpResourcesBridge.getFromTypeName(typeFullName);
		assertThat(file.getName(), is("NestedNamespaces.cs"));
	}
}