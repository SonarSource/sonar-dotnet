/*
 * Sonar C# Plugin :: Dependency
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
package org.sonar.plugins.csharp.dependency;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.SonarIndex;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.PersistenceMode;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.graph.Cycle;
import org.sonar.graph.CycleDetector;
import org.sonar.graph.Dsm;
import org.sonar.graph.DsmTopologicalSorter;
import org.sonar.graph.Edge;
import org.sonar.graph.MinimumFeedbackEdgeSetSolver;
import org.sonar.plugins.csharp.api.CSharpConstants;

import com.google.common.collect.Lists;

/**
 * Copy/pasted from the original DSM descorator.
 * Should be removed when the deisgn API will be multi language
 */
public class CSharpDsmDecorator implements Decorator {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpDsmDecorator.class);
  
  // hack as long as DecoratorContext does not implement SonarIndex
  private SonarIndex index;

  public CSharpDsmDecorator(SonarIndex index) {
    this.index = index;
  }

  public boolean shouldExecuteOnProject(Project project) {
    return CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey());
  }

  public void decorate(final Resource resource, DecoratorContext context) {
    if (shouldDecorateResource(resource, context)) {
      Collection<Resource> subResources = getSubResources(context);
      
      Dsm<Resource> dsm = getDsm(subResources);
      
      Measure measure = new Measure(CoreMetrics.DEPENDENCY_MATRIX, DsmSerializer.serialize(dsm));
      //Measure measure = new Measure(CoreMetrics.DEPENDENCY_MATRIX, (String)null);
      measure.setPersistenceMode(PersistenceMode.DATABASE);
      context.saveMeasure(measure);
    }
  }

  private Collection<Resource> getSubResources(DecoratorContext context) {
    List<Resource> result = new ArrayList<Resource>();
    for(DecoratorContext childContext : context.getChildren()){
      result.add(childContext.getResource());
    }
    
    return result;
  }

  private Dsm<Resource> getDsm(Collection<Resource> subResources) {
    CycleDetector<Resource> cycleDetector = new CycleDetector<Resource>(index, subResources);
    Set<Cycle> cycles = cycleDetector.getCycles();

    MinimumFeedbackEdgeSetSolver solver = new MinimumFeedbackEdgeSetSolver(cycles);
    Set<Edge> feedbackEdges = solver.getEdges();

    Dsm<Resource> dsm = new Dsm<Resource>(index, subResources, feedbackEdges);
    try{
    DsmTopologicalSorter.sort(dsm);
    }catch(IllegalStateException ise){
      LOG.warn("Dsm sort", ise);
    }
    return dsm;
  }

  private boolean shouldDecorateResource(Resource resource, DecoratorContext context) {
    return context.getMeasure(CoreMetrics.DEPENDENCY_MATRIX) == null;
  }
}
