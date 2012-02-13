/*
 * Sonar C# Plugin :: Gallio
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.plugins.csharp.gallio;

import java.util.Arrays;
import java.util.List;

import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Formula;
import org.sonar.api.measures.FormulaContext;
import org.sonar.api.measures.FormulaData;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.MeasureUtils;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.ResourceUtils;


/**
 * Strongly inspired by class AverageComplexityFormula
 * Formula used to produce measures for metric ASSERT_PER_TEST
 * 
 * @author Alexandre Victoor
 *
 */
public class AverageAssertFormula implements Formula {

  public List<Metric> dependsUponMetrics() {
    return Arrays.asList(new Metric[] { TestMetrics.COUNT_ASSERTS, CoreMetrics.TESTS });
  }

  public Measure calculate(FormulaData data, FormulaContext context) {
    if (!(shouldDecorateResource(data, context))) {
      return null;
    }
    
    if (ResourceUtils.isFile(context.getResource())) {
      Double tests = MeasureUtils.getValue(data.getMeasure(CoreMetrics.TESTS), null);
      Double asserts = MeasureUtils.getValue(data.getMeasure(TestMetrics.COUNT_ASSERTS), null);
      if ((tests != null) && (asserts != null) && (tests.doubleValue() > 0.0D)) {
        return new Measure(context.getTargetMetric(), Double.valueOf(asserts.doubleValue() / tests.doubleValue()));
      }
    } else {
      double totalTests = 0.0D;
      double totalAsserts = 0.0D;
      boolean hasApplicableChildren = false;
   
      for (FormulaData childrenData : data.getChildren()) {
        Double childrenByTests = MeasureUtils.getValue(childrenData.getMeasure(CoreMetrics.TESTS), null);
        Double childrenAsserts = MeasureUtils.getValue(childrenData.getMeasure(TestMetrics.COUNT_ASSERTS), null);
        if ((childrenAsserts != null) && (childrenByTests != null) && (childrenByTests.doubleValue() > 0.0D)) {
          totalTests += childrenByTests.doubleValue();
          totalAsserts += childrenAsserts.doubleValue();
          hasApplicableChildren = true;
        }
      }
      if (hasApplicableChildren) {
        return new Measure(context.getTargetMetric(), Double.valueOf(totalAsserts / totalTests));
      }
    }
    return null;
  }
  
  private boolean shouldDecorateResource(FormulaData data, FormulaContext context) {
    return (!(MeasureUtils.hasValue(data.getMeasure(context.getTargetMetric()))));
  }

}
