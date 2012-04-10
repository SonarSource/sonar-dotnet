/*
 * Sonar C# Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps;

import static org.mockito.Matchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;

/*
 * Not too much test effort here as CSharpDsmDecorator will be removed once the design part of Sonar will be available in the API.
 */
public class CSharpDsmDecoratorTest {

  private CSharpDsmDecorator dsmDecorator;
  private DecoratorContext context;

  @Before
  public void init() {
    dsmDecorator = new CSharpDsmDecorator(null);
    context = mock(DecoratorContext.class);
  }

  @Test
  public void shouldNotBeExecuted() {
    // given that
    when(context.getMeasure(CoreMetrics.DEPENDENCY_MATRIX)).thenReturn(new Measure(CoreMetrics.DEPENDENCY_MATRIX, "..."));

    // when
    dsmDecorator.decorate(null, context);

    // then
    verify(context, never()).saveMeasure(any(Measure.class));
  }

  @Test
  public void shouldExecute() throws Exception {
    // when
    dsmDecorator.decorate(null, context);

    // then
    verify(context, times(1)).saveMeasure(any(Measure.class));
  }

}
