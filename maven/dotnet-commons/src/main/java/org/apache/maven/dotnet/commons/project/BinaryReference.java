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

package org.apache.maven.dotnet.commons.project;

public class BinaryReference {

  private String assemblyName; 
  private String version; 
  private String scope;
  
  public String getAssemblyName() {
    return assemblyName;
  }

  public void setAssemblyName(String assemblyName) {
    this.assemblyName = assemblyName;
  }

  public String getVersion() {
    return version;
  }

  public void setVersion(String version) {
    this.version = version;
  }

  public String getScope() {
    return scope;
  }

  public void setScope(String scope) {
    this.scope = scope;
  }

  @Override
  public String toString() {
    return assemblyName+':'+version;
  }

  @Override
  public int hashCode() {
    final int prime = 31;
    int result = 1;
    result = prime * result
        + ((assemblyName == null) ? 0 : assemblyName.hashCode());
    result = prime * result + ((scope == null) ? 0 : scope.hashCode());
    result = prime * result + ((version == null) ? 0 : version.hashCode());
    return result;
  }

  @Override
  @SuppressWarnings("all")
  public boolean equals(Object obj) {
    if (this == obj)
      return true;
    if (obj == null)
      return false;
    if (getClass() != obj.getClass())
      return false;
    BinaryReference other = (BinaryReference) obj;
    if (assemblyName == null) {
      if (other.assemblyName != null)
        return false;
    } else if (!assemblyName.equals(other.assemblyName))
      return false;
    if (scope == null) {
      if (other.scope != null)
        return false;
    } else if (!scope.equals(other.scope))
      return false;
    if (version == null) {
      if (other.version != null)
        return false;
    } else if (!version.equals(other.version))
      return false;
    return true;
  }
  
  
}
