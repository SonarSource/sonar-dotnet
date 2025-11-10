/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import java.nio.file.Path;
import java.util.Objects;
import org.sonar.api.scanner.fs.InputProject;

public class RoslynReport {

  private final InputProject project;
  private final Path reportPath;

  public RoslynReport(InputProject project, Path reportPath) {
    this.project = project;
    this.reportPath = reportPath;
  }

  public InputProject getProject() {
    return project;
  }

  public Path getReportPath() {
    return reportPath;
  }

  @Override
  public boolean equals(Object o) {
    if (this == o) {
      return true;
    }
    if (o == null || getClass() != o.getClass()) {
      return false;
    }
    RoslynReport that = (RoslynReport) o;
    return Objects.equals(project, that.project) &&
      Objects.equals(reportPath, that.reportPath);
  }

  @Override
  public int hashCode() {
    return Objects.hash(project, reportPath);
  }
}
