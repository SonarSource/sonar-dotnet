/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on Apr 20, 2010
 */
package org.sonar.plugin.dotnet.srcmon.model;

/**
 * Represent a measure distribution.
 * 
 * @author Jose CHILLAN Apr 20, 2010
 */
public class Distribution {
  private final DistributionClassification classification;
  private final int[] segments;

  /**
   * Constructs a @link{Distribution}.
   */
  public Distribution(DistributionClassification classification) {
    this.classification = classification;
    int countClasses = classification.getCountClasses();
    this.segments = new int[countClasses];
  }

  /**
   * Adds a value in the distribution
   * 
   * @param value
   */
  public void addEntry(int value) {
    int segmentNumber = classification.getSegmentNumber(value);

    // Increments the segment
    segments[segmentNumber]++;
  }

  /**
   * Combines a distribution supposes to contain the same limits.
   * 
   * @param distribution
   *          the other distribution to combine
   */
  public void combine(Distribution distribution) {
    if (!classification.equals(distribution.classification)) {
      throw new IllegalArgumentException(
          "Impossible to combine two distribution that don't share the same distribution");
    }
    // Adds the result of both distribution in each segment
    for (int idx = 0; idx < segments.length; idx++) {
      segments[idx] += distribution.segments[idx];
    }
  }

  /**
   * Generates the sonar representation of the distribution.
   * 
   * @return a sonar database compliant representation of the distribution
   */
  public String toSonarRepresentation() {
    StringBuilder buffer = new StringBuilder();
    int countStops = segments.length - 1;
    for (int idx = 0; idx < countStops; idx++) {
      int limit = classification.getStop(idx);
      buffer.append(limit);
      buffer.append("=");
      buffer.append(segments[idx]);
      buffer.append(";");
    }
    int lastLimit = classification.getStop(countStops);
    buffer.append(lastLimit);
    buffer.append("=");
    buffer.append(segments[countStops]);
    return buffer.toString();
  }
}
