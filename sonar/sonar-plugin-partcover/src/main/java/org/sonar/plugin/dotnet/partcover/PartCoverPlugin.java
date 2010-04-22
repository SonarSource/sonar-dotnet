/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
package org.sonar.plugin.dotnet.partcover;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;

/**
 * The plugin that handles the code coverage with PartCover.
 * @author Jose CHILLAN Feb 18, 2010
 */
public class PartCoverPlugin
  implements Plugin
{

  /**
   * The plugin key.
   */
  public static final String KEY = "partcover";

  public PartCoverPlugin()
  {
  }

  public String getDescription()
  {
    return "A plugin that compute the code coverage in .Net with Part Cover";
  }

  /**
   * The extentions required by the plugin.
   * @return
   */
  public List<Class<? extends Extension>> getExtensions()
  {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(PartCoverSensor.class);
    list.add(CoverageMetrics.class);
    list.add(PartCoverPluginHandler.class);

    return list;
  }

  public String getKey()
  {
    return KEY;
  }

  public String getName()
  {
    return "PartCover plugin";
  }

  @Override
  public String toString()
  {
    return getKey();
  }
}
