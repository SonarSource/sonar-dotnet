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
 * Created on Apr 30, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.api.measures.Metrics;

/**
 * Metrics specific to the Gallio plugin that don't exist in base Sonar.
 * 
 * @author Jose CHILLAN Apr 30, 2009
 */
public class GallioMetrics implements Metrics {
  public static final Metric COUNT_ASSERTS = new Metric("count_asserts",
      "Count Assert", "The number of asserts performed by the unit tests",
      Metric.ValueType.INT, -1, false, CoreMetrics.DOMAIN_TESTS);

  public List<Metric> getMetrics() {
    ArrayList<Metric> metrics = new ArrayList<Metric>();
    metrics.add(COUNT_ASSERTS);
    return metrics;

  }

}
