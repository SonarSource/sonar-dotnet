/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

class BranchPoint {
  private final int offset;
  private final int offsetEnd;
  private final int startLine;
  private final int hits;
  // identifier for code path
  private final int path;
  private final String filePath;
  private final String coverageIdentifier;

  public BranchPoint(String filePath, int startLine, int offset, int offsetEnd, int path, int hits, String coverageIdentifier) {
    this.filePath = filePath;
    this.startLine = startLine;
    this.offset = offset;
    this.offsetEnd = offsetEnd;
    this.path = path;
    this.hits = hits;
    this.coverageIdentifier = coverageIdentifier;
  }

  public int getStartLine() {
    return startLine;
  }

  public int getHits() {
    return hits;
  }

  public String getFilePath() {
    return filePath;
  }

  public String getCoverageIdentifier() {
    return coverageIdentifier;
  }

  public String getUniqueKey() {
    return String.format("%s-%d-%d-%d-%d", filePath, startLine, offset, offsetEnd, path);
  }
}
