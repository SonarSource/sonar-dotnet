/*
 * .NET tools :: Gallio Runner
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
package org.sonar.dotnet.tools.gallio;

/**
 * Runner types supported by the Gallio command line.
 * See the official Gallio doc : 
 * http://www.gallio.org/wiki/doku.php?id=tools:gallio_test_runners
 * 
 * @author Alexandre Victoor
 */
public enum GallioRunnerType {

  ISOLATED_APP_DOMAIN("IsolatedAppDomain"), 
  ISOLATED_PROCESS("IsolatedProcess"), 
  LOCAL("Local"), 
  NCOVER("NCover3");
  
  private final String value;

  private GallioRunnerType(String value) {
    this.value = value;
  }

  public String getValue() {
    return value;
  }
  
  public static GallioRunnerType parse(String param) {
    GallioRunnerType[] values = GallioRunnerType.values();
    for (GallioRunnerType gallioRunnerType : values) {
      if (gallioRunnerType.value.equals(param)) {
        return gallioRunnerType;
      }
    }
    throw new IllegalArgumentException(param + " is not a valid Gallio runner type");
  }
  
}
