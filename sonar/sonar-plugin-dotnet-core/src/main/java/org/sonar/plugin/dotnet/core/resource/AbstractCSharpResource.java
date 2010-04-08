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
 * Created on Sep 1, 2009
 *
 */
package org.sonar.plugin.dotnet.core.resource;

import org.sonar.api.resources.AbstractResource;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.CSharp;

/**
 * A base class for C# resources.
 * @author Jose CHILLAN Sep 1, 2009
 */
@SuppressWarnings("unchecked")
public abstract class AbstractCSharpResource<PARENT extends Resource> extends AbstractResource<PARENT>
{

  /**
   * Constructs a @link{AbstractCSharpResource}.
   * @param key
   * @param scope
   * @param qualifier
   */
  public AbstractCSharpResource(String key, String scope, String qualifier)
  {
    super(key, scope, qualifier);
    setLanguage(CSharp.INSTANCE);
  }

  /**
   * Constructs a @link{AbstractCSharpResource}.
   * @param scope
   * @param qualifier
   */
  public AbstractCSharpResource(String scope, String qualifier)
  {
    super(scope, qualifier);
    setLanguage(CSharp.INSTANCE);
  }

}
