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

import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import org.sonar.api.scanner.ScannerSide;

/**
 * Collects information about what types of files are in each project (MAIN, TEST, both or none).
 * The invoker should make sure that no duplicates are added.
 */
@ScannerSide
public class ProjectTypeCollector {
  private final List<Project> projects = new ArrayList<>();

  void addProjectInfo(boolean hasMainFiles, boolean hasTestFiles) {
    projects.add(new Project(hasMainFiles, hasTestFiles));
  }

  Optional<String> getSummary() {
    int onlyMain = 0;
    int onlyTest = 0;
    int mixed = 0;
    int projectsCount = 0;
    int noFiles = 0;

    for (Project project : projects) {
      projectsCount++;
      if (project.hasMainFiles) {
        if (project.hasTestFiles) {
          mixed++;
        } else {
          onlyMain++;
        }
      } else {
        // no main files
        if (project.hasTestFiles) {
          onlyTest++;
        } else {
          noFiles++;
        }
      }
    }

    // The top-level module (an artificial module for doing global (solution level) operations) has no files.
    // To avoid counting it, we remove it.
    if (noFiles > 0) {
      noFiles--;
      projectsCount--;
    }

    return createMessage(projectsCount, onlyMain, onlyTest, mixed, noFiles);
  }

  private static Optional<String> createMessage(int projectsCount, int onlyMain, int onlyTest, int mixed, int noFiles) {
    if (projectsCount == 0) {
      return Optional.empty();
    }
    StringBuilder stringBuilder = new StringBuilder(String.format("Found %d MSBuild projects.", projectsCount));
    if (onlyMain > 0) {
      stringBuilder.append(String.format(" %d MAIN project(s).", onlyMain));
    }
    if (onlyTest > 0) {
      stringBuilder.append(String.format(" %d TEST project(s).", onlyTest));
    }
    if (mixed > 0) {
      stringBuilder.append(String.format(" %d with both MAIN and TEST files.", mixed));
    }
    if (noFiles > 0) {
      stringBuilder.append(String.format(" %d with no MAIN or TEST files.", noFiles));
    }

    return Optional.of(stringBuilder.toString());
  }

  // .NET Project (Scanner "module")
  private static class Project {
    final boolean hasMainFiles;
    final boolean hasTestFiles;

    Project(boolean hasMainFiles, boolean hasTestFiles) {
      this.hasMainFiles = hasMainFiles;
      this.hasTestFiles = hasTestFiles;
    }
  }
}
