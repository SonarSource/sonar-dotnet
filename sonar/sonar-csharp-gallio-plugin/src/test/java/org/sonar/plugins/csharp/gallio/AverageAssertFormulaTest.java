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

import com.google.common.collect.Lists;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FormulaContext;
import org.sonar.api.measures.FormulaData;
import org.sonar.api.measures.Measure;
import org.sonar.api.resources.JavaFile;

import java.util.List;

import static junit.framework.Assert.assertNull;
import static org.hamcrest.CoreMatchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class AverageAssertFormulaTest {

  private FormulaContext context;
  private FormulaData data;

  @Before
  public void before() {
    context = mock(FormulaContext.class);
    when(context.getTargetMetric()).thenReturn(TestMetrics.ASSERT_PER_TEST);
    data = mock(FormulaData.class);
  }

  @Test
  public void testAverageCalculation() {
    List<FormulaData> childrenData = Lists.newArrayList();
    FormulaData data1 = mock(FormulaData.class);
    childrenData.add(data1);
    when(data1.getMeasure(CoreMetrics.TESTS)).thenReturn(new Measure(CoreMetrics.TESTS, 43.0));
    when(data1.getMeasure(TestMetrics.COUNT_ASSERTS)).thenReturn(new Measure(CoreMetrics.TESTS, 107.0));

    FormulaData data2 = mock(FormulaData.class);
    childrenData.add(data2);
    when(data2.getMeasure(CoreMetrics.TESTS)).thenReturn(new Measure(CoreMetrics.TESTS, 127.0));
    when(data2.getMeasure(TestMetrics.COUNT_ASSERTS)).thenReturn(new Measure(CoreMetrics.TESTS, 233.0));

    when(data.getChildren()).thenReturn(childrenData);

    Measure measure = new AverageAssertFormula().calculate(data, context);

    assertThat(measure.getValue(), is(2.0));
  }

  @Test
  public void testWhenNoChildrenMeasures() {
    List<FormulaData> childrenData = Lists.newArrayList();
    when(data.getChildren()).thenReturn(childrenData);
    Measure measure = new AverageAssertFormula().calculate(data, context);
    assertNull(measure);
  }

  @Test
  public void testWhenNoComplexityMeasures() {
    List<FormulaData> childrenData = Lists.newArrayList();
    FormulaData data1 = mock(FormulaData.class);
    childrenData.add(data1);
    when(data1.getMeasure(CoreMetrics.TESTS)).thenReturn(new Measure(CoreMetrics.TESTS, 43.0));

    when(data.getChildren()).thenReturn(childrenData);
    Measure measure = new AverageAssertFormula().calculate(data, context);

    assertNull(measure);
  }

  @Test
  public void testWhenNoByMetricMeasures() {
    List<FormulaData> childrenData = Lists.newArrayList();
    FormulaData data1 = mock(FormulaData.class);
    childrenData.add(data1);
    when(data1.getMeasure(TestMetrics.COUNT_ASSERTS)).thenReturn(new Measure(TestMetrics.COUNT_ASSERTS, 43.0));

    when(data.getChildren()).thenReturn(childrenData);
    Measure measure = new AverageAssertFormula().calculate(data, context);

    assertNull(measure);
  }

  @Test
  public void testWhenMixedMetrics() {
    List<FormulaData> childrenData = Lists.newArrayList();
    FormulaData data1 = mock(FormulaData.class);
    childrenData.add(data1);
    when(data1.getMeasure(CoreMetrics.TESTS)).thenReturn(new Measure(CoreMetrics.TESTS, 43.0));
    when(data1.getMeasure(TestMetrics.COUNT_ASSERTS)).thenReturn(new Measure(CoreMetrics.TESTS, 107.0));

    FormulaData data2 = mock(FormulaData.class);
    childrenData.add(data2);
    when(data2.getMeasure(CoreMetrics.PARAGRAPHS)).thenReturn(new Measure(CoreMetrics.PARAGRAPHS, 127.0));
    when(data2.getMeasure(TestMetrics.COUNT_ASSERTS)).thenReturn(new Measure(CoreMetrics.TESTS, 233.0));

    when(data.getChildren()).thenReturn(childrenData);

    Measure measure = new AverageAssertFormula().calculate(data, context);

    assertThat(measure.getValue(), is(2.5));
  }

  @Test
  public void testCalculationForFIle() {
    when(data.getMeasure(TestMetrics.COUNT_ASSERTS)).thenReturn(new Measure(TestMetrics.COUNT_ASSERTS, 60.0));
    when(data.getMeasure(CoreMetrics.TESTS)).thenReturn(new Measure(CoreMetrics.TESTS, 20.0));
    when(context.getResource()).thenReturn(new JavaFile("foo"));

    Measure measure = new AverageAssertFormula().calculate(data, context);
    assertThat(measure.getValue(), is(3.0));
  }
}
