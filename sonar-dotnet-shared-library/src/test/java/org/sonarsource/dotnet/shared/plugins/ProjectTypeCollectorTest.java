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

// These tests use "SUT" (System Under Test) to name the object being tested.
public class ProjectTypeCollectorTest {
  private static final String LANG_NAME = "LANG";

  @Test
  public void withNoProjects() {
    ProjectTypeCollector sut = new ProjectTypeCollector();
    assertThat(sut.getSummary(LANG_NAME)).isEmpty();
  }

  @Test
  public void withNoFiles() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addProjectWithNoFiles(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 1 MSBuild LANG project: 1 with no MAIN nor TEST files.");
  }

  @Test
  public void withOnlyMainFiles() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addMainProject(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 1 MSBuild LANG project: 1 MAIN project.");
  }

  @Test
  public void withOnlyTestFile() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addTestProject(sut);
    addTestProject(sut);
    addTestProject(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 3 MSBuild LANG projects: 3 TEST projects.");
  }

  @Test
  public void withBothTypes() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addProjectWithBothTypes(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 1 MSBuild LANG project: 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_test_and_main() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addTestProject(sut);
    addMainProject(sut);
    addTestProject(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 3 MSBuild LANG projects: 1 MAIN project. 2 TEST projects.");
  }

  @Test
  public void mixedProjects_test_and_both() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addTestProject(sut);
    addProjectWithBothTypes(sut);
    addTestProject(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 3 MSBuild LANG projects: 2 TEST projects. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_main_and_both() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addMainProject(sut);
    addProjectWithBothTypes(sut);
    addMainProject(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 3 MSBuild LANG projects: 2 MAIN projects. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_test_none() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addTestProject(sut);
    addTestProject(sut);
    addProjectWithNoFiles(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 3 MSBuild LANG projects: 2 TEST projects. 1 with no MAIN nor TEST files.");
  }

  @Test
  public void mixedProjects_main_none() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addProjectWithNoFiles(sut);
    addMainProject(sut);
    addMainProject(sut);
    addMainProject(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 4 MSBuild LANG projects: 3 MAIN projects. 1 with no MAIN nor TEST files.");
  }

  @Test
  public void mixedProjects_all_types() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addTestProject(sut);
    addMainProject(sut);
    addProjectWithBothTypes(sut);

    assertThat(sut.getSummary(LANG_NAME)).hasValue("Found 3 MSBuild LANG projects: 1 MAIN project. 1 TEST project. 1 with both MAIN and TEST files.");
  }

  @Test
  public void mixedProjects_all_types_null_or_empty_language_name() {
    ProjectTypeCollector sut = new ProjectTypeCollector();

    addTestProject(sut);
    addMainProject(sut);
    addProjectWithBothTypes(sut);

    // this should not happen in real life, but we want to make sure we don't fail hard
    assertThat(sut.getSummary(null)).hasValue("Found 3 MSBuild null projects: 1 MAIN project. 1 TEST project. 1 with both MAIN and TEST files.");
    assertThat(sut.getSummary("")).hasValue("Found 3 MSBuild  projects: 1 MAIN project. 1 TEST project. 1 with both MAIN and TEST files.");
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

  private void addProjectWithBothTypes(ProjectTypeCollector projectTypeCollector) {
    projectTypeCollector.addProjectInfo(true, true);
  }
}
