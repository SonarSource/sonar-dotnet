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
package org.sonarsource.dotnet.shared.sarif;

import javax.annotation.CheckForNull;
import javax.annotation.Nullable;

public class Location {
  private String absolutePath;
  private String message;
  private int startLine;
  private int startColumn;
  private int endLine;
  private int endColumn;

  Location(String absolutePath, @Nullable String message, int startLine, int startColumn, int endLine, int endColumn) {
    this.absolutePath = absolutePath;
    this.message = message;
    this.startLine = startLine;
    this.startColumn = startColumn;
    this.endLine = endLine;
    this.endColumn = endColumn;
  }

  public String getAbsolutePath() {
    return absolutePath;
  }

  @CheckForNull
  public String getMessage() {
    return message;
  }

  public int getStartLine() {
    return startLine;
  }

  public int getStartColumn() {
    return startColumn;
  }

  public int getEndLine() {
    return endLine;
  }

  public int getEndColumn() {
    return endColumn;
  }

  @Override
  public boolean equals(Object o) {
    if (this == o) {
      return true;
    }
    if (o == null || getClass() != o.getClass()) {
      return false;
    }

    Location location = (Location) o;

    if (startLine != location.startLine ||
        startColumn != location.startColumn ||
        endLine != location.endLine ||
        endColumn != location.endColumn) {
      return false;
    }
    return ((absolutePath != null) ? absolutePath.equals(location.absolutePath) : (location.absolutePath == null)) &&
        ((message != null) ? message.equals(location.message) : (location.message == null));
  }

  @Override
  public int hashCode() {
    int result = absolutePath != null ? absolutePath.hashCode() : 0;
    result = 31 * result + (message != null ? message.hashCode() : 0);
    result = 31 * result + startLine;
    result = 31 * result + startColumn;
    result = 31 * result + endLine;
    result = 31 * result + endColumn;
    return result;
  }

  @Override
  public String toString() {
    return "Location [absolutePath=" + absolutePath + ", message=" + message + ", startLine=" + startLine + ", startColumn=" +
        startColumn + ", endLine=" + endLine + ", endColumn=" + endColumn + "]";
  }

}
