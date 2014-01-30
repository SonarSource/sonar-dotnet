/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
package com.sonar.csharp.squid.api;

import org.sonar.squid.measures.AggregationFormula;
import org.sonar.squid.measures.CalculatedMetricFormula;
import org.sonar.squid.measures.MetricDef;
import org.sonar.squid.measures.SumAggregationFormula;

/**
 * Metrics computed for the C# language.
 */
public enum CSharpMetric implements MetricDef {
  FILES, CLASSES, INTERFACES, DELEGATES, STRUCTS, ENUMS, METHODS, LINES, LINES_OF_CODE, STATEMENTS, ACCESSORS, COMPLEXITY, COMMENT_BLANK_LINES, COMMENT_LINES, PUBLIC_API,
  PUBLIC_DOC_API;

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
