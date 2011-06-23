/*
 * Sonar C# Plugin :: Core
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

package org.sonar.plugins.csharp.api.squid;

import org.sonar.squid.measures.AggregationFormula;
import org.sonar.squid.measures.CalculatedMetricFormula;
import org.sonar.squid.measures.MetricDef;
import org.sonar.squid.measures.NoAggregationFormula;
import org.sonar.squid.measures.SumAggregationFormula;

/**
 * Metrics computed for the C# language.
 */
public enum CSharpMetric implements MetricDef {
  FILES, NAMESPACES, CLASSES, INTERFACES, DELEGATES, STRUCTS, ENUMS, METHODS, LINES, LINES_OF_CODE, STATEMENTS, ACCESSORS, COMPLEXITY, COMMENT_BLANK_LINES, COMMENTED_OUT_CODE_LINES, COMMENT_LINES, PUBLIC_API, PUBLIC_DOC_API;

  private double initValue = 0;

  private CalculatedMetricFormula formula = null;

  private AggregationFormula aggregationFormula = new SumAggregationFormula();

  private boolean aggregateIfThereIsAlreadyAValue = true;

  CSharpMetric() {
  }

  CSharpMetric(boolean aggregateIfThereIsAlreadyAValue) {
    this.aggregateIfThereIsAlreadyAValue = aggregateIfThereIsAlreadyAValue;
  }

  CSharpMetric(AggregationFormula aggregationFormula) {
    this.aggregationFormula = aggregationFormula;
  }

  CSharpMetric(CalculatedMetricFormula formula) {
    this.formula = formula;
  }

  public double getInitValue() {
    return initValue;
  }

  /**
   * {@inheritDoc}
   */
  public String getName() {
    return name();
  }

  /**
   * {@inheritDoc}
   */
  public boolean isCalculatedMetric() {
    return formula != null;
  }

  /**
   * {@inheritDoc}
   */
  public boolean aggregateIfThereIsAlreadyAValue() {
    return aggregateIfThereIsAlreadyAValue;
  }

  /**
   * {@inheritDoc}
   */
  public boolean isThereAggregationFormula() {
    return !(aggregationFormula instanceof NoAggregationFormula);
  }

  /**
   * {@inheritDoc}
   */
  public CalculatedMetricFormula getCalculatedMetricFormula() {
    return formula;
  }

  public AggregationFormula getAggregationFormula() {
    return aggregationFormula;
  }

}
