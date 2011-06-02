/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

/*
 * Created on Jun 4, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import static org.sonar.plugin.dotnet.gallio.Constants.*;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;

/**
 * A Unit test plugin for Sonar C# based on the Gallio project..
 * 
 * @author Jose CHILLAN Jun 4, 2009
 */
@Properties({
  @Property(
      key = GALLIO_MODE_KEY,
      defaultValue = GALLIO_DEFAULT_MODE,
      name = ".net coverage activation mode",
      description = "Possible values : enable, skip, reuseReport",
      project = true,
      module = false,
      global = true),
  @Property(
      key = GALLIO_REPORT_KEY,
      defaultValue = GALLIO_REPORT_XML,
      name = "Name of the .net coverage report file",
      description = "Name of the .net coverage report file used when reuse report mode is activated",
      project = true,
      module = false,
      global = true)
})
public class GallioPlugin implements Plugin {
  
  public String getDescription() {
    return "A plugin that collects the Gallio test results";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(GallioSensor.class);
    list.add(GallioMetrics.class);
    list.add(GallioMavenPluginHandler.class);
    list.add(CSharpFileLocator.class);
    return list;
  }

  public String getKey() {
    return "gallio";
  }

  public String getName() {
    return "[.net] Gallio";
  }

  @Override
  public String toString() {
    return getKey();
  }

}
