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
 * Created on Jun 4, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;

/**
 * A Unit test plugin for Sonar C# based on the Gallio project..
 * 
 * @author Jose CHILLAN Jun 4, 2009
 */
public class GallioPlugin implements Plugin {
  /**
   * Constructs a @link{GallioPlugin}.
   */
  public GallioPlugin() {
  }

  public String getDescription() {
    return "A plugin that collects the Gallio test results";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(GallioSensor.class);
    list.add(GallioMetrics.class);
    list.add(UnitTestDecorator.class);
    return list;
  }

  public String getKey() {
    return "gallio";
  }

  public String getName() {
    return "Gallio Plugin";
  }

  @Override
  public String toString() {
    return getKey();
  }

}
