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
 * Created on May 5, 2009
 */
package org.sonar.plugin.dotnet.core;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;

/**
 * The plugin that declares the C# language.
 * @author Jose CHILLAN May 5, 2009
 */
public class CSharpPlugin
  implements Plugin
{
  public CSharpPlugin()
  {
  }

  public String getDescription()
  {
    return "A plugin that declares the CSharp language";
  }

  /**
   * Gets the extensions for the C# language
   * @return
   */
  public List<Class<? extends Extension>> getExtensions()
  {
    ArrayList<Class<? extends Extension>> extensions = new ArrayList<Class<? extends Extension>>();
    extensions.add(CSharp.class);
    extensions.add(CSharpSourceImporter.class);
    return extensions;
  }

  public String getKey()
  {
    return "csharp";
  }

  public String getName()
  {
    return "CSharp plugin";
  }

}
