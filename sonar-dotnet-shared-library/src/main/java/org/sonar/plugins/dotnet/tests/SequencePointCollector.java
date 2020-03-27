/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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

import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;

public class SequencePointCollector {
  private static final Logger LOG = Loggers.get(SequencePointCollector.class);
  private Set<SequencePoint> sequencePoints = new HashSet<>();

  public void add(SequencePoint sequencePoint){
    // we only want to count covered sequences when these are on the same line
    if (sequencePoint.getStartLine() == sequencePoint.getLineEnd()) {
      sequencePoints.add(sequencePoint);
    }
  }

  public void publishCoverage(Coverage coverage){
    Map<String, List<SequencePoint>> sequencePointsPerFile = sequencePoints.stream().collect(Collectors.groupingBy(SequencePoint::getFilePath));
    for (Map.Entry<String, List<SequencePoint>> fileSequencePoints : sequencePointsPerFile.entrySet()){
      publishCoverage(coverage, fileSequencePoints.getKey(), fileSequencePoints.getValue());
    }
  }

  private void publishCoverage(Coverage coverage, String filePath, List<SequencePoint> sequencePoints){
    Map<Integer, List<SequencePoint>> sequencePointsPerLine = sequencePoints.stream().collect(Collectors.groupingBy(SequencePoint::getStartLine));

    LOG.trace("Found coverage information about '{}' lines having single-line sequence points for file '{}'", sequencePointsPerLine.size(), filePath);

    for (Map.Entry<Integer, List<SequencePoint>> lineSequencePoints : sequencePointsPerLine.entrySet()){
      if (lineSequencePoints.getValue().size() > 1){
        int line = lineSequencePoints.getKey();

        List<SequencePoint> linePoints = lineSequencePoints.getValue();
        int coveredPoints = (int)linePoints.stream().filter(point -> point.getHits() > 0).count();

        coverage.addBranchCoverage(filePath, new BranchCoverage(line, linePoints.size(), coveredPoints));
      }
    }
  }
}
