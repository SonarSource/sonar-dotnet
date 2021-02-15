/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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

import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class ProjectTypeCollectorTest {

  @Test
  public void withNoProjects() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    assertThat(underTest.getSummary()).isEmpty();
  }

  @Test
  public void withNoFiles_skip() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(false, false);

    assertThat(underTest.getSummary()).isEmpty();
  }

  @Test
  public void withNoFiles_twoModules_logOne() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(false, false);
    underTest.addProjectInfo(false, false);

    assertThat(underTest.getSummary()).hasValue("Found 1 MSBuild projects. 1 with no MAIN or TEST files.");
  }

  @Test
  public void withOnlyMainFiles() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(true, false);

    assertThat(underTest.getSummary()).hasValue("Found 1 MSBuild projects. 1 MAIN project(s).");
  }

  @Test
  public void withOnlyTestFile() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(false, true);
    underTest.addProjectInfo(false, true);
    underTest.addProjectInfo(false, true);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 3 TEST project(s).");
  }

  @Test
  public void withBothTypes_calledOnce_returnsEmpty() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(true, true);

    assertThat(underTest.getSummary()).hasValue("Found 1 MSBuild projects. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_test_and_main() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(false, true);
    underTest.addProjectInfo(true, false);
    underTest.addProjectInfo(false, true);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 1 MAIN project(s). 2 TEST project(s).");
  }

  @Test
  public void mixedProjects_test_and_both() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(false, true);
    underTest.addProjectInfo(true, true);
    underTest.addProjectInfo(false, true);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 2 TEST project(s). 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_main_and_both() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(true, false);
    underTest.addProjectInfo(true, true);
    underTest.addProjectInfo(true, false);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 2 MAIN project(s). 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_test_none() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    underTest.addProjectInfo(false, true);
    underTest.addProjectInfo(false, true);
    // this will be skipped - considered as top-level module
    underTest.addProjectInfo(false, false);
    underTest.addProjectInfo(false, false);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 2 TEST project(s). 1 with no MAIN or TEST files.");
  }

  @Test
  public void mixedProjects_main_none() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    // this will be skipped, considered as top-level
    underTest.addProjectInfo(false, false);
    underTest.addProjectInfo(false, false);
    underTest.addProjectInfo(true, false);
    underTest.addProjectInfo(true, false);
    underTest.addProjectInfo(true, false);

    assertThat(underTest.getSummary()).hasValue("Found 4 MSBuild projects. 3 MAIN project(s). 1 with no MAIN or TEST files.");
  }

  @Test
  public void mixedProjects_all_types() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();
    // this will be skipped, considered as top-level module
    underTest.addProjectInfo(false, false);
    underTest.addProjectInfo(false, true);
    underTest.addProjectInfo(true, false);
    underTest.addProjectInfo(true, true);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 1 MAIN project(s). 1 TEST project(s). 1 with both MAIN and TEST files.");
  }
}