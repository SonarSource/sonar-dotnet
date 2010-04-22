/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
/*
 * Created on Apr 22, 2010
 *
 */
package org.sonar.plugin.dotnet.srcmon.model;

import java.util.Arrays;

/**
 * A pattern of distribution. It contains the stops that limits the classes.
 * @author Jose CHILLAN Apr 22, 2010
 */
public class DistributionClassification
{
  private final int[] stops;

  /**
   * The Sonar classification used for method complexity
   */
  public final static DistributionClassification METHOD_COMPLEXITY= new DistributionClassification(1, 2, 4, 6, 8, 10, 12);
  
  /**
   * The Sonar classification used for classes complexity
   */
  public final static DistributionClassification CLASS_COMPLEXITY = new DistributionClassification(0, 5, 10, 20, 30, 60, 90);
  /**
   * Constructs a @link{DistributionPattern}.
   * @param stops the limits (stops) for the classification, in ascending order
   */
  public DistributionClassification(int ... stops)
  {
    super();
    this.stops = stops;
  }
  
  /**
   * Gets the segment number of a value in a distribution classification.
   * @param value the value to classify
   * @return
   */
  public int getSegmentNumber(int value)
  {
    // Finds the interval for the value
    for (int idx = 1; idx < stops.length; idx++)
    {
      int limit = stops[idx];
      if (value < limit)
      {
        // The segment has been found
        return idx - 1;
      }      
    }
    // This is the last segment
    return stops.length - 1; 
  }
  
  /**
   * Gets a specific stop value
   * @param idx the index of the stop
   * @return the value of the stop
   */
  public int getStop(int idx)
  {
    return stops[idx];
  }
  /**
   * Gets the count of classes, including the last unlimited one.
   * @return the number of classes for this distribution
   */
  public int getCountClasses()
  {
    return stops.length;
  }

  /**
   * @return
   */
  @Override
  public int hashCode()
  {
    final int prime = 31;
    int result = 1;
    result = prime * result + Arrays.hashCode(stops);
    return result;
  }

  /**
   * @param obj
   * @return
   */
  @Override
  public boolean equals(Object obj)
  {
    if (this == obj)
      return true;
    if (obj == null)
      return false;
    if (getClass() != obj.getClass())
      return false;
    DistributionClassification other = (DistributionClassification) obj;
    if (!Arrays.equals(stops, other.stops))
      return false;
    return true;
  }
  
  
}
