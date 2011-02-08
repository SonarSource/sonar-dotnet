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
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.coverage;

import static org.sonar.plugin.dotnet.coverage.Constants.*;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.coverage.stax.CoverageResultStaxParser;

/**
 * The plugin that handles the code coverage with PartCover or NCover.
 * 
 * @author Jose CHILLAN Feb 18, 2010
 */
@Properties({
  @Property(
      key = COVERAGE_MODE_KEY,
      defaultValue = COVERAGE_DEFAULT_MODE,
      name = ".net coverage activation mode",
      description = "Possible values : enable, skip, reuseReport",
      project = true,
      module = false,
      global = true),
  @Property(
      key = COVERAGE_REPORT_KEY,
      defaultValue = COVERAGE_REPORT_XML,
      name = "Name of the .net coverage report file",
      description = "Name of the .net coverage report file used when reuse report mode is activated",
      project = true,
      module = false,
      global = true)
})
public class CoveragePlugin implements Plugin {

  /**
   * The plugin key.
   */
  public static final String KEY = "coverage";

  public String getDescription() {
    return "A plugin that compute the code coverage in .Net with PartCover or NCover";
  }

  /**
   * The extensions required by the plugin.
   * 
   * @return
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(CoverageSensor.class);
    list.add(CoverageMetrics.class);
    list.add(CoveragePluginHandler.class);
    list.add(CoverageResultStaxParser.class);
    list.add(CSharpFileLocator.class);
    return list; 
  }

  public String getKey() {
    return KEY;
  }

  public String getName() {
    return "[.net] Coverage";
  }

  @Override
  public String toString() {
    return getKey();
  }
}
