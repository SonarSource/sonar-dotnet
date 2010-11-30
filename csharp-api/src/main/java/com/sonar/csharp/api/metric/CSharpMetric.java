/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api.metric;

import org.sonar.squid.measures.AggregationFormula;
import org.sonar.squid.measures.CalculatedMetricFormula;
import org.sonar.squid.measures.MetricDef;
import org.sonar.squid.measures.NoAggregationFormula;
import org.sonar.squid.measures.SumAggregationFormula;

/**
 * Metrics computed for the C# language.
 */
public enum CSharpMetric implements MetricDef {
  PUBLIC_API, PUBLIC_DOC_API, METHODS, LINES_OF_CODE, STATEMENTS, LINES, FILES, COMMENT_BLANK_LINES, COMMENTED_OUT_CODE_LINES, COMMENT_LINES, COMPLEXITY;

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

  public String getName() {
    return name();
  }

  public boolean isCalculatedMetric() {
    return formula != null;
  }

  public boolean aggregateIfThereIsAlreadyAValue() {
    return aggregateIfThereIsAlreadyAValue;
  }

  public boolean isThereAggregationFormula() {
    return !(aggregationFormula instanceof NoAggregationFormula);
  }

  public CalculatedMetricFormula getCalculatedMetricFormula() {
    return formula;
  }

  public AggregationFormula getAggregationFormula() {
    return aggregationFormula;
  }

}
