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
package org.sonar.plugins.dotnet.tests;

import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;

public class Coverage {

  private static final int MINIMUM_FILE_LINES = 100;
  private static final int GROW_FACTOR = 2;
  private static final int SPECIAL_HITS_NON_EXECUTABLE = -1;
  private final Map<String, int[]> hitsByLineAndFile = new HashMap<>();

  void addHits(String file, int line, int hits) {
    int[] oldHitsByLine = hitsByLineAndFile.get(file);

    if (oldHitsByLine == null) {
      oldHitsByLine = new int[Math.max(line, MINIMUM_FILE_LINES)];
      for (int i = 0; i < oldHitsByLine.length; i++) {
        oldHitsByLine[i] = SPECIAL_HITS_NON_EXECUTABLE;
      }
      hitsByLineAndFile.put(file, oldHitsByLine);
    } else if (oldHitsByLine.length < line) {
      int[] tmp = new int[line * GROW_FACTOR];
      System.arraycopy(oldHitsByLine, 0, tmp, 0, oldHitsByLine.length);
      for (int i = oldHitsByLine.length; i < tmp.length; i++) {
        tmp[i] = SPECIAL_HITS_NON_EXECUTABLE;
      }
      oldHitsByLine = tmp;
      hitsByLineAndFile.put(file, oldHitsByLine);
    }

    int i = line - 1;
    if (oldHitsByLine[i] == SPECIAL_HITS_NON_EXECUTABLE) {
      oldHitsByLine[i] = 0;
    }
    oldHitsByLine[i] += hits;
  }

  public Set<String> files() {
    return hitsByLineAndFile.keySet();
  }

  Map<Integer, Integer> hits(String file) {
    int[] oldHitsByLine = hitsByLineAndFile.get(file);
    if (oldHitsByLine == null) {
      return Collections.emptyMap();
    }

    Map<Integer, Integer> result = new HashMap<>();
    for (int i = 0; i < oldHitsByLine.length; i++) {
      if (oldHitsByLine[i] != SPECIAL_HITS_NON_EXECUTABLE) {
        result.put(i + 1, oldHitsByLine[i]);
      }
    }

    return result;
  }

  void mergeWith(Coverage otherCoverage) {
    Map<String, int[]> other = otherCoverage.hitsByLineAndFile;

    for (Map.Entry<String, int[]> entry : other.entrySet()) {
      String file = entry.getKey();
      int[] otherHitsByLine = entry.getValue();

      for (int i = otherHitsByLine.length - 1; i >= 0; i--) {
        addHits(file, i + 1, otherHitsByLine[i]);
      }
    }
  }

}
