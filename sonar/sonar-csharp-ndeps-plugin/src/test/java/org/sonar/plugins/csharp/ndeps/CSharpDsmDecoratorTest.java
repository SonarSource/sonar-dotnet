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
