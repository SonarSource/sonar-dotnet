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
 * Created on Sep 1, 2009
 */
package org.sonar.plugin.dotnet.core.resource;

import org.apache.commons.lang.builder.ToStringBuilder;
import org.sonar.api.resources.Language;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.CSharp;

/**
 * A base class for C# resources.
 * 
 * @author Jose CHILLAN Sep 1, 2009
 */
@SuppressWarnings("unchecked")
public abstract class AbstractCSharpResource<PARENT extends Resource> extends Resource<PARENT>
{
  private String   name;
  private String   description;
  private String   scope;
  private String   qualifier;
  private Language language;

  protected AbstractCSharpResource(String key, String scope, String qualifier)
  {

    this.scope = scope;
    setKey(key);
    setLanguage(CSharp.INSTANCE);
    setQualifier(qualifier);
  }

  /**
   * Constructs a @link{AbstractCSharpResource}.
   * 
   * @param scope
   * @param qualifier
   */
  public AbstractCSharpResource(String scope, String qualifier)
  {
    this.scope = scope;
    setLanguage(CSharp.INSTANCE);
    setQualifier(qualifier);
  }

  public String getName()
  {
    return name;
  }

  public Resource<? extends PARENT> setName(String name)
  {
    this.name = name;
    return this;
  }

  public String getDescription()
  {
    return description;
  }

  public Resource<? extends PARENT> setDescription(String description)
  {
    this.description = description;
    return this;
  }

  public String getScope()
  {
    return scope;
  }

  public void setQualifier(String qualifier)
  {
    this.qualifier = qualifier;
  }

  public String getQualifier()
  {
    return qualifier;
  }

  public Language getLanguage()
  {
    return language;
  }

  public Resource<? extends PARENT> setLanguage(Language language)
  {
    this.language = language;
    return this;
  }

  public PARENT getParent()
  {
    return null;
  }

  @Override
  public String toString()
  {
    return new ToStringBuilder(this).append("key", getKey()).append("scope", scope).append("qualifier", qualifier).append("name", name)
                                    .append("language", language).append("description", description).toString();
  }

}
