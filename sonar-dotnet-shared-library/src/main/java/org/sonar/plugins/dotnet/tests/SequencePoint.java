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
