/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

public class SequencePoint {
  private final int lineStart;
  private final int lineEnd;
  private final int hits;
  private String filePath;

  SequencePoint(String filePath, int lineStart, int lineEnd, int hits) {
    this(lineStart, lineEnd, hits);
    this.filePath = filePath;
  }

  SequencePoint(int lineStart, int lineEnd, int hits){
    this.lineStart = lineStart;
    this.lineEnd = lineEnd;
    this.hits = hits;
  }

  public String getFilePath() {
    return filePath;
  }

  public int getStartLine() {
    return lineStart;
  }

  public int getLineEnd() {
    return lineEnd;
  }

  public int getHits() {
    return hits;
  }
}
