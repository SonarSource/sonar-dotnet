/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api;

import org.sonar.squid.measures.AggregationFormula;
import org.sonar.squid.measures.CalculatedMetricFormula;
import org.sonar.squid.measures.MetricDef;
import org.sonar.squid.measures.SumAggregationFormula;

/**
 * Metrics computed for the C# language.
 */
public enum CSharpMetric implements MetricDef {
  FILES, CLASSES, INTERFACES, DELEGATES, STRUCTS, ENUMS, METHODS, LINES, LINES_OF_CODE, STATEMENTS, ACCESSORS, COMPLEXITY, COMMENT_BLANK_LINES,
  COMMENTED_OUT_CODE_LINES, COMMENT_LINES, PUBLIC_API, PUBLIC_DOC_API;

  public double getInitValue() {
    return 0;
  }

  public String getName() {
    return name();
  }

  public boolean isCalculatedMetric() {
    return false;
  }

  public boolean aggregateIfThereIsAlreadyAValue() {
    return true;
  }

  public boolean isThereAggregationFormula() {
    return true;
  }

  public CalculatedMetricFormula getCalculatedMetricFormula() {
    return null;
  }

  public AggregationFormula getAggregationFormula() {
    return new SumAggregationFormula();
  }

}
