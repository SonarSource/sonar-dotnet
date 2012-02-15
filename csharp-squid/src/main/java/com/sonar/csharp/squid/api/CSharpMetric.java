/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.squid.api;

import org.sonar.squid.measures.*;

/**
 * Metrics computed for the C# language.
 */
public enum CSharpMetric implements MetricDef {
  FILES, NAMESPACES, CLASSES, INTERFACES, DELEGATES, STRUCTS, ENUMS, METHODS, LINES, LINES_OF_CODE, STATEMENTS, ACCESSORS, COMPLEXITY, COMMENT_BLANK_LINES, COMMENTED_OUT_CODE_LINES, COMMENT_LINES, PUBLIC_API, PUBLIC_DOC_API;

  private final double initValue = 0;

  private final CalculatedMetricFormula formula = null;

  private final AggregationFormula aggregationFormula = new SumAggregationFormula();

  private final boolean aggregateIfThereIsAlreadyAValue = true;

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
