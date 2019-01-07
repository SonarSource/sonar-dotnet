/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class ReportPathCollectorTest {
  private ReportPathCollector underTest = new ReportPathCollector();

  @Test
  public void should_save_roslyn_report_paths() {
    RoslynReport p1 = new RoslynReport(null, Paths.get("p1"));
    RoslynReport p2 = new RoslynReport(null, Paths.get("p2"));
    underTest.addRoslynDirs(Collections.singletonList(p1));
    underTest.addRoslynDirs(Collections.singletonList(p2));
    assertThat(underTest.roslynDirs()).containsOnly(p1, p2);
    assertThat(underTest.protobufDirs()).isEmpty();
  }

  @Test
  public void should_save_proto_report_paths() {
    Path p1 = Paths.get("p1");
    Path p2 = Paths.get("p2");
    underTest.addProtobufDirs(Collections.singletonList(p1));
    underTest.addProtobufDirs(Collections.singletonList(p2));
    assertThat(underTest.protobufDirs()).containsOnly(p1, p2);
    assertThat(underTest.roslynDirs()).isEmpty();
  }
}
