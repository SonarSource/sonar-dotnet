/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

class ConditionData {
  private final String format;
  private final Location location;
  private final int startLine;
  private final int hits;
  // identifier for code path
  private final int path;
  private final String filePath;
  private final String coverageIdentifier;

  public ConditionData(String format, String filePath, int startLine, Location location, int path, int hits, String coverageIdentifier) {
    this.format = format;
    this.filePath = filePath;
    this.startLine = startLine;
    this.location = location;
    this.path = path;
    this.hits = hits;
    this.coverageIdentifier = coverageIdentifier;
  }

  public String getFormat() {
    return format;
  }

  public int getStartLine() {
    return startLine;
  }

  public int getHits() {
    return hits;
  }

  public int getLocationStart() {
    return location.start;
  }

  public int getLocationEnd() {
    return location.end;
  }

  public int getPath() {
    return path;
  }

  public String getFilePath() {
    return filePath;
  }

  public String getCoverageIdentifier() {
    return coverageIdentifier;
  }

  public String getUniqueKey() {
    return String.format("%s-%d-%d-%d-%d", filePath, startLine, location.start, location.end, path);
  }

  record Location(int start, int end) {
  }
}
