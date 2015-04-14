/*
 * SonarQube C# Plugin
 * Copyright (C) 2014 SonarSource
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
package org.sonar.plugins.csharp;

import com.google.common.base.Charsets;
import com.google.common.io.Files;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;

public class ZipTest {

  @Rule
  public TemporaryFolder tmp = new TemporaryFolder();

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void test() throws Exception {
    File out = tmp.getRoot();
    assertThat(out.listFiles()).hasSize(0);

    new Zip(new File("src/test/resources/ZipTest/sample.zip")).unzip(out);
    assertThat(out.listFiles()).hasSize(4);
    assertThat(Files.toString(new File(out, "a.txt"), Charsets.UTF_8)).isEqualTo("hello");
    assertThat(Files.toString(new File(out, "b.txt"), Charsets.UTF_8)).isEqualTo("");

    File foo = new File(out, "foo");
    assertThat(foo).exists();
    assertThat(foo.listFiles()).isEmpty();

    File bar = new File(out, "bar");
    assertThat(bar.listFiles()).hasSize(2);
    assertThat(Files.toString(new File(bar, "c.txt"), Charsets.UTF_8)).isEqualTo("world");

    File baz = new File(out, "bar/baz");
    assertThat(baz).exists();
    assertThat(baz.listFiles()).isEmpty();
  }

  @Test
  public void non_existing() {
    thrown.expectMessage("java.io.FileNotFoundException");
    thrown.expectMessage("non_existing.zip");

    new Zip(new File("src/test/resources/ZipTest/non_existing.zip")).unzip(tmp.getRoot());
  }

}
