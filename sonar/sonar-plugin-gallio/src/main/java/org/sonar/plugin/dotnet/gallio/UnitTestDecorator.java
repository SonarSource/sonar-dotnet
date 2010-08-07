package org.sonar.plugin.dotnet.gallio;

import java.util.Arrays;
import java.util.Collection;
import java.util.List;

import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.MeasureUtils;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.resources.ResourceUtils;

/**
 * Copy/paste of the sonar UnitTestDecorator. Workaround for a bug present in
 * sonar 2.1.X where the surefire plugin insert a bad unit test metric for non
 * java projects.
 * 
 * @author Alexandre Victoor
 * 
 */
public class UnitTestDecorator implements Decorator {

  @DependedUpon
  public List<Metric> generatesMetrics() {
    return Arrays.asList(CoreMetrics.TEST_EXECUTION_TIME, CoreMetrics.TESTS,
        CoreMetrics.TEST_ERRORS, CoreMetrics.TEST_FAILURES,
        CoreMetrics.TEST_SUCCESS_DENSITY);
  }

  public boolean shouldExecuteOnProject(Project project) {
    return "sln".equals(project.getPackaging());
  }

  public boolean shouldDecorateResource(Resource resource) {
    return ResourceUtils.isUnitTestClass(resource)
        || !ResourceUtils.isEntity(resource);
  }

  public void decorate(Resource resource, DecoratorContext context) {
    if (shouldDecorateResource(resource)) {
      sumChildren(context, CoreMetrics.TEST_EXECUTION_TIME);
      sumChildren(context, CoreMetrics.SKIPPED_TESTS);
      Double tests = sumChildren(context, CoreMetrics.TESTS);
      Double errors = sumChildren(context, CoreMetrics.TEST_ERRORS);
      Double failures = sumChildren(context, CoreMetrics.TEST_FAILURES);

      if (isPositive(tests, true) && isPositive(errors, false)
          && isPositive(failures, false)) {
        Double errorsAndFailuresRatio = (errors + failures) * 100.0 / tests;
        context.saveMeasure(CoreMetrics.TEST_SUCCESS_DENSITY,
            100.0 - errorsAndFailuresRatio);
      }
    }
  }

  private boolean isPositive(Double d, boolean strict) {
    return d != null && (strict ? d > 0.0 : d >= 0.0);
  }

  private Double sumChildren(DecoratorContext jobContext, Metric metric) {
    Collection<Measure> childrenMeasures = jobContext
        .getChildrenMeasures(metric);
    if (childrenMeasures != null && childrenMeasures.size() > 0) {
      Double sum = 0.0;
      boolean hasChildrenMeasures = false;
      for (Measure measure : childrenMeasures) {
        if (MeasureUtils.hasValue(measure)) {
          sum += measure.getValue();
          hasChildrenMeasures = true;
        }
      }
      if (hasChildrenMeasures) {
        jobContext.saveMeasure(metric, sum);
        return sum;
      }
    }
    return null;
  }

  @Override
  public String toString() {
    return getClass().getSimpleName();
  }

}
