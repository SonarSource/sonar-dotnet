/*
 * Sonar .NET Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps.decorators;

import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.design.Dependency;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.resources.Resource;
import org.sonar.api.resources.ResourceUtils;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;

import java.util.Collection;

public class CouplingDecorator extends DecoratorSupport {

  public CouplingDecorator(MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment);
  }

  @SuppressWarnings("rawtypes")
  public void decorate(final Resource resource, DecoratorContext context) {
    if (shouldDecorateResource(context)) {
      Collection<Dependency> incomingDependencies = context.getIncomingDependencies();
      Collection<Dependency> outgoingDependencies = context.getOutgoingDependencies();
      if (incomingDependencies != null) {
        context.saveMeasure(CoreMetrics.AFFERENT_COUPLINGS, (double) incomingDependencies.size());
      }
      if (outgoingDependencies != null) {
        context.saveMeasure(CoreMetrics.EFFERENT_COUPLINGS, (double) outgoingDependencies.size());
      }
    }
  }

  private boolean shouldDecorateResource(DecoratorContext context) {
    Resource<?> resource = context.getResource();
    return ResourceUtils.isFile(resource);
  }

}
