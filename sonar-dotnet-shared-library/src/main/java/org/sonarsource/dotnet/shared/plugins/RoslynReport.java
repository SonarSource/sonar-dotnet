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
import java.util.Objects;
import org.sonar.api.batch.fs.InputModule;

public class RoslynReport {

  private final InputModule module;
  private final Path reportPath;

  public RoslynReport(InputModule module, Path reportPath) {
    this.module = module;
    this.reportPath = reportPath;
  }

  public InputModule getModule() {
    return module;
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
    return Objects.equals(module, that.module) &&
      Objects.equals(reportPath, that.reportPath);
  }

  @Override
  public int hashCode() {
    return Objects.hash(module, reportPath);
  }
}
