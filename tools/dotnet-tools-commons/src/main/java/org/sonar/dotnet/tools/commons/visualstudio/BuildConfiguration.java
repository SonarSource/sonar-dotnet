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
package org.sonar.dotnet.tools.commons.visualstudio;

import org.apache.commons.lang.StringUtils;
import org.apache.commons.lang.builder.EqualsBuilder;
import org.apache.commons.lang.builder.HashCodeBuilder;


public class BuildConfiguration {

  public static final String DEFAULT_PLATFORM = "Any CPU"; 
  
  private final String name;
  private final String platform;
  
  public BuildConfiguration(String name, String platform) {
    this.name = name;
    if (StringUtils.isEmpty(platform)) {
      this.platform = DEFAULT_PLATFORM;
    } else {
      this.platform = platform;
    }
    
  }
  
  public BuildConfiguration(String name) {
    this.name = name;
    this.platform = DEFAULT_PLATFORM;
  }

  public String getName() {
    return name;
  }

  
  public String getPlatform() {
    return platform;
  }

  @Override
  public int hashCode() {
    return HashCodeBuilder.reflectionHashCode(this);
  }

  @Override
  public boolean equals(Object obj) {
    return EqualsBuilder.reflectionEquals(this, obj);
  }

  @Override
  public String toString() {
    return StringUtils.remove(name + "|" + platform, " ");
  }
  
}
