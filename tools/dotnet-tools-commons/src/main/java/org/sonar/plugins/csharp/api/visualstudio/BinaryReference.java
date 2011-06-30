/*
 * .NET tools :: Commons
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
package org.sonar.plugins.csharp.api.visualstudio;

import org.apache.commons.lang.builder.EqualsBuilder;
import org.apache.commons.lang.builder.HashCodeBuilder;

/**
 * Model class for dotnet binary dependencies.
 * These objects are created using the information 
 * contained in the csproj files. 
 * 
 * @author Alexandre Victoor
 */
public class BinaryReference {

  private String assemblyName;
  private String version;
  private String scope;

  public String getAssemblyName() {
    return assemblyName;
  }

  void setAssemblyName(String assemblyName) {
    this.assemblyName = assemblyName;
  }

  public String getVersion() {
    return version;
  }

  void setVersion(String version) {
    this.version = version;
  }

  public String getScope() {
    return scope;
  }

  void setScope(String scope) {
    this.scope = scope;
  }

  @Override
  public String toString() {
    return assemblyName + ':' + version + ':' + scope;
  }

  @Override
  public int hashCode() {
    return new HashCodeBuilder().append(assemblyName).append(scope).append(version).toHashCode();
  }

  @Override
  @SuppressWarnings("all")
  public boolean equals(Object obj) {
    if (obj == null) {
      return false;
    }
    if (obj == this) {
      return true;
    }
    if (obj.getClass() != getClass()) {
      return false;
    }
    BinaryReference ref = (BinaryReference) obj;
    return new EqualsBuilder().append(assemblyName, ref.assemblyName).append(scope, ref.scope).append(version, ref.version).isEquals();
  }

}
