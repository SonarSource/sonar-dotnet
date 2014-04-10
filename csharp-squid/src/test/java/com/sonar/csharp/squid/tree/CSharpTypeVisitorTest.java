/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
package com.sonar.csharp.squid.tree;

import com.google.common.collect.ImmutableList;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceClass;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.csharp.squid.parser.RuleTest;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.api.Grammar;
import org.sonar.squidbridge.AstScanner;
import org.apache.commons.io.FileUtils;
import org.junit.Test;
import org.sonar.squidbridge.api.SourceCode;
import org.sonar.squidbridge.api.SourceProject;
import org.sonar.squidbridge.indexer.QueryByType;

import java.io.File;
import java.nio.charset.Charset;
import java.util.Collection;

import static org.fest.assertions.Assertions.assertThat;

public class CSharpTypeVisitorTest extends RuleTest {

  @Test
  public void testScanFile() {
    AstScanner<Grammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")));
    scanner.scanFile(readFile("/tree/TypesAllInOneFile.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    assertThat(project.getInt(CSharpMetric.CLASSES)).isEqualTo(10);
    assertThat(project.getInt(CSharpMetric.INTERFACES)).isEqualTo(1);
    assertThat(project.getInt(CSharpMetric.DELEGATES)).isEqualTo(1);
    assertThat(project.getInt(CSharpMetric.STRUCTS)).isEqualTo(2);
    assertThat(project.getInt(CSharpMetric.ENUMS)).isEqualTo(1);

    Collection<SourceCode> squidClasses = scanner.getIndex().search(new QueryByType(SourceClass.class));
    assertThat(keys(squidClasses)).containsOnly(
      "Foo.Class",
      "Foo.Struct.InnerClass",
      "Foo.Bar.Baz.Class1",
      "Foo.Bar.Baz.Class1.Class2",
      "Foo.Bar.Baz.Class1.Class2.Class3",
      "Foo.Class4",
      "GenericClass",
      "GenericClass<1>",
      "GenericClass<2>",
      "GenericClass<5>");

    Collection<SourceCode> squidTypes = scanner.getIndex().search(new QueryByType(SourceType.class));
    assertThat(keys(squidTypes)).containsOnly(
      "Foo.Class.InnerStruct",
      "Foo.Struct",
      "Foo.Enum",
      "Bar.Interface",
      "Bar.Delegate");
  }

  public Collection<String> keys(Collection<SourceCode> squidClasses) {
    ImmutableList.Builder<String> builder = ImmutableList.builder();
    for (SourceCode squidClass : squidClasses) {
      builder.add(squidClass.getKey());
    }
    return builder.build();
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
