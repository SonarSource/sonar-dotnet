/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.coverage;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.api.measures.Metrics;

/**
 * Defines coverage specific metrics.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class CoverageMetrics
  implements Metrics
{
  /**
   * The number of effective lines of code
   */
  public static final Metric ELOC = new Metric("eloc",
                                               "Effective lines of code",
                                               "The number of lines of code with statements",
                                               Metric.ValueType.INT,
                                               -1,
                                               false,
                                               CoreMetrics.DOMAIN_SIZE);

  /**
   * Constructs a @link{CovergeMetrics}.
   */
  public CoverageMetrics()
  {
  }

  public List<Metric> getMetrics()
  {
    ArrayList<Metric> metrics = new ArrayList<Metric>();
    metrics.add(ELOC);
    return metrics;

  }

}
