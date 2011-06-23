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

package org.sonar.plugins.csharp.api;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.notNullValue;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Resource;
import org.sonar.plugins.csharp.api.squid.source.SourceMember;
import org.sonar.plugins.csharp.api.squid.source.SourceType;
import org.sonar.squid.api.SourceFile;

public class CSharpResourcesBridgeTest {

  private static CSharpResourcesBridge cSharpResourcesBridge;

  @BeforeClass
  public static void init() {
    SourceFile sourceFile = new SourceFile("/temp/Fake.cs", "Fake.cs");
    SourceType sourceType = new SourceType("MyNamespace.MyClass", "MyClass");
    SourceType sourceTypeWithoutNamespace = new SourceType("MyClassWithoutNamespace", "MyClassWithoutNamespace");
    SourceMember sourceMember = new SourceMember(sourceType, "GetFoo", 0);
    sourceFile.addChild(sourceType);
    sourceFile.addChild(sourceTypeWithoutNamespace);
    sourceType.addChild(sourceMember);

    File sonarFile = mock(File.class);
    when(sonarFile.getName()).thenReturn("Fake.cs");

    cSharpResourcesBridge = new CSharpResourcesBridge();
    cSharpResourcesBridge.indexFile(sourceFile, sonarFile);
  }

  @Test
  public void testGetFromType() {
    Resource<?> file = cSharpResourcesBridge.getFromTypeName("MyNamespace", "MyClass");
    assertThat(file, notNullValue());
    assertThat(file.getName(), is("Fake.cs"));

    file = cSharpResourcesBridge.getFromTypeName("MyNamespace.MyClass");
    assertThat(file, notNullValue());
    assertThat(file.getName(), is("Fake.cs"));
  }

  @Test
  public void testGetFromTypeWithoutNamespace() {
    Resource<?> file = cSharpResourcesBridge.getFromTypeName("", "MyClassWithoutNamespace");
    assertThat(file, notNullValue());
    assertThat(file.getName(), is("Fake.cs"));
  }

  @Test(expected = IllegalStateException.class)
  public void testLocking() {
    cSharpResourcesBridge.lock();
    cSharpResourcesBridge.indexFile(null, null);
  }

  @Test
  public void testGetFromMember() {
    Resource<?> file = cSharpResourcesBridge.getFromMemberName("MyNamespace.MyClass#GetFoo");
    assertThat(file.getName(), is("Fake.cs"));
  }

}
