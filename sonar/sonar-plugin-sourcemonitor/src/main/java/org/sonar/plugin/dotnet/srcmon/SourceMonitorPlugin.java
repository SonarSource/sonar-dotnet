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

/*
 * Created on Apr 30, 2009
 */
package org.sonar.plugin.dotnet.srcmon;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;

/**
 * A Sonar plugin for Source Monitor data.
 * 
 * @author Jose CHILLAN Apr 30, 2009
 */
public class SourceMonitorPlugin implements Plugin {
  public final static String SOURCE_MONITOR_REPORT = "metrics-report.xml";

  /**
   * Constructs a @link{SourceMonitorPlugin}.
   */
  public SourceMonitorPlugin() {
  }

  public String getDescription() {
    return "A plugin that collects the SourceMonitor metrics";
  }

  @Override
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(DotnetSourceMetrics.class);
    list.add(SourceMonitorSensor.class);
    list.add(SourceMonitorPluginHandler.class);
    list.add(CSharpFileLocator.class);
    // list.add(LineOfCodeJob.class);
    // list.add(NonCommendLineOfCodeJob.class);
    // list.add(StatementJob.class);
    // list.add(ComplexityJob.class);

    return list;
  }

  public String getKey() {
    return "sourcemonitor";
  }

  public String getName() {
    return "SourceMonitor Plugin";
  }

}
