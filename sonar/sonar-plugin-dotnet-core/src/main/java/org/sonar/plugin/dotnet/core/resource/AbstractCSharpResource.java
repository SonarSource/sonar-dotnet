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
public abstract class AbstractCSharpResource<PARENT extends Resource> extends
    Resource<PARENT> {
  private String name;
  private String description;
  private String scope;
  private String qualifier;

  protected AbstractCSharpResource(String key, String scope, String qualifier) {

    this.scope = scope;
    setKey(key);
    this.qualifier = qualifier;
  }

  /**
   * Constructs a @link{AbstractCSharpResource}.
   * 
   * @param scope
   * @param qualifier
   */
  public AbstractCSharpResource(String scope, String qualifier) {
    this.scope = scope;
    this.qualifier = qualifier;
  }

  public String getName() {
    return name;
  }

  public Resource<? extends PARENT> setName(String name) {
    this.name = name;
    return this;
  }

  public String getDescription() {
    return description;
  }

  public Resource<? extends PARENT> setDescription(String description) {
    this.description = description;
    return this;
  }

  public String getScope() {
    return scope;
  }

  public String getQualifier() {
    return qualifier;
  }

  public Language getLanguage() {
    return CSharp.INSTANCE;
  }

  public PARENT getParent() {
    return null;
  }
  // HACK to avoid classloader issues 
  @Override
  public boolean equals(Object o) {
    if (this == o) {
      return true;
    }
    if (o == null ||  !getClass().getName().equals(o.getClass().getName()) ) {
      return false;
    }

    Resource resource = (Resource) o;
    return getKey().equals(resource.getKey());

  }
  
  @Override
  public int hashCode() {
    return super.hashCode();
  }

  @Override
  public String toString() {
    return new ToStringBuilder(this).append("key", getKey())
        .append("scope", scope).append("qualifier", qualifier)
        .append("name", name)
        .append("description", description).toString();
  }

}
