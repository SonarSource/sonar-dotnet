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

    assertThat(underTest.getSummary()).isEmpty();
  }

  @Test
  public void withNoFiles_twoModules_logOne() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addProjectWithNoFiles(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 1 MSBuild project. 1 with no MAIN nor TEST files.");
  }

  @Test
  public void withOnlyMainFiles() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addMainProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 1 MSBuild project. 1 MAIN project.");
  }

  @Test
  public void withOnlyTestFile() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addTestProject(underTest);
    addTestProject(underTest);
    addTestProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 3 TEST projects.");
  }

  @Test
  public void withBothTypes_calledOnce_returnsEmpty() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addMixedProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 1 MSBuild project. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_test_and_main() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addTestProject(underTest);
    addMainProject(underTest);
    addTestProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 1 MAIN project. 2 TEST projects.");
  }

  @Test
  public void mixedProjects_test_and_both() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addTestProject(underTest);
    addMixedProject(underTest);
    addTestProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 2 TEST projects. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_main_and_both() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addMainProject(underTest);
    addMixedProject(underTest);
    addMainProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 2 MAIN projects. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_test_none() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addTestProject(underTest);
    addTestProject(underTest);
    addProjectWithNoFiles(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 2 TEST projects. 1 with no MAIN nor TEST files.");
  }

  @Test
  public void mixedProjects_main_none() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addProjectWithNoFiles(underTest);
    addMainProject(underTest);
    addMainProject(underTest);
    addMainProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 4 MSBuild projects. 3 MAIN projects. 1 with no MAIN nor TEST files.");
  }

  @Test
  public void mixedProjects_all_types() {
    ProjectTypeCollector underTest = new ProjectTypeCollector();

    addTestProject(underTest);
    addMainProject(underTest);
    addMixedProject(underTest);

    assertThat(underTest.getSummary()).hasValue("Found 3 MSBuild projects. 1 MAIN project. 1 TEST project. 1 with both MAIN and TEST files.");
  }

  private void addProjectWithNoFiles(ProjectTypeCollector projectTypeCollector) {
    projectTypeCollector.addProjectInfo(false, false);
  }

  private void addTestProject(ProjectTypeCollector projectTypeCollector) {
    projectTypeCollector.addProjectInfo(false, true);
  }

  private void addMainProject(ProjectTypeCollector projectTypeCollector) {
    projectTypeCollector.addProjectInfo(true, false);
  }

  private void addMixedProject(ProjectTypeCollector projectTypeCollector) {
    projectTypeCollector.addProjectInfo(true, true);
  }
}
