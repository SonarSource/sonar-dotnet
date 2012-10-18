/*
 * Sonar .NET Plugin :: Core
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
package org.sonar.plugins.dotnet.api;

import com.google.common.collect.Maps;
import org.sonar.api.BatchExtension;

import java.util.Map;

/**
 * Class used to find a .NET resource bridge based on a given language.
 */
public class DotNetResourceBridges implements BatchExtension {

  private Map<String, DotNetResourceBridge> bridges = Maps.newHashMap();

  /**
   * Emtpy-parameter constructor (required by Pico) 
   */
  public DotNetResourceBridges() {
  }

  /**
   * Constructor
   */
  public DotNetResourceBridges(DotNetResourceBridge[] dotNetResourceBridges) {
    for (DotNetResourceBridge dotNetResourceBridge : dotNetResourceBridges) {
      bridges.put(dotNetResourceBridge.getLanguageKey(), dotNetResourceBridge);
    }
  }

  /**
   * Returns the {@link DotNetResourceBridge} corresponding to the given language key, or NULL is none is available.
   */
  public DotNetResourceBridge getBridge(String languageKey) {
    return bridges.get(languageKey);
  }

}
