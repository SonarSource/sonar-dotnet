/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
package org.sonar.plugins.dotnet.tests.coverage;

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
