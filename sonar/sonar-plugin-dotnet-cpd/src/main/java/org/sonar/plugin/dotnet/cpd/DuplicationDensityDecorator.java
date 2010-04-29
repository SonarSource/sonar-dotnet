/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

package org.sonar.plugin.dotnet.cpd;

import java.util.Arrays;
import java.util.List;
import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.measures.*;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;

public class DuplicationDensityDecorator
    implements Decorator
{

    public DuplicationDensityDecorator()
    {
    }

    public List dependsUponMetrics()
    {
        return Arrays.asList(new Metric[] {
            CoreMetrics.NCLOC, CoreMetrics.COMMENT_LINES, CoreMetrics.DUPLICATED_LINES, CoreMetrics.LINES
        });
    }

    public Metric generatesMetric()
    {
        return CoreMetrics.DUPLICATED_LINES_DENSITY;
    }

    public boolean shouldExecuteOnProject(Project project)
    {
        return true;
    }

    public void decorate(Resource resource, DecoratorContext context)
    {
        Measure nbDuplicatedLines = context.getMeasure(CoreMetrics.DUPLICATED_LINES);
        if(nbDuplicatedLines == null)
            return;
        Double divisor = getNbLinesFromLocOrNcloc(context);
        if(divisor != null && divisor.doubleValue() > 0.0D)
            context.saveMeasure(CoreMetrics.DUPLICATED_LINES_DENSITY, calculate(nbDuplicatedLines.getValue(), divisor));
    }

    private Double getNbLinesFromLocOrNcloc(DecoratorContext context)
    {
        Measure nbLoc = context.getMeasure(CoreMetrics.LINES);
        if(nbLoc != null)
            return nbLoc.getValue();
        Measure nbNcloc = context.getMeasure(CoreMetrics.NCLOC);
        if(nbNcloc != null)
        {
            Measure nbComments = context.getMeasure(CoreMetrics.COMMENT_LINES);
            Double nbLines = nbNcloc.getValue();
            return Double.valueOf(nbComments == null ? nbLines.doubleValue() : nbLines.doubleValue() + nbComments.getValue().doubleValue());
        } else
        {
            return null;
        }
    }

    protected Double calculate(Double dividend, Double divisor)
    {
        Double result = Double.valueOf((100D * dividend.doubleValue()) / divisor.doubleValue());
        if(result.doubleValue() < 100D)
            return result;
        else
            return Double.valueOf(100D);
    }
}
