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
package org.sonar.plugins.dotnet.core;

import com.google.common.collect.Maps;
import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;

import java.util.Map;

/**
 * Class used to handle .Net languages.
 * <br><br>
 * Note: this is not possible to dynamically discover .NET languages because this DotNetLanguages class
 * is required at InstantiationStrategy.PER_BATCH (for the {@link VisualStudioProjectBuilder}), but Sonar 
 * Language classes are not available at that time.
 * <br><br>
 * This is why this class is not part of the API.
 */
public class DotNetLanguages implements BatchExtension, ServerExtension { // NOSONAR Required to be able to mock the class

  private static final String[] DOTNET_LANGUAGE_KEYS = new String[] {"cs", "vbnet"};
  private static final Map<String, String> FILE_SUFFIXE_TO_LANGUAGE_KEY_MAP = Maps.newHashMap();

  static {
    FILE_SUFFIXE_TO_LANGUAGE_KEY_MAP.put("cs", "cs");
    FILE_SUFFIXE_TO_LANGUAGE_KEY_MAP.put("vb", "vbnet");
  }

  private DotNetLanguages() {
  }

  /**
   * Returns true only if the given language Sonar key represents a .NET language.
   */
  public static boolean isDotNetLanguage(String languageKey) {
    for (int i = 0; i < DOTNET_LANGUAGE_KEYS.length; i++) {
      if (DOTNET_LANGUAGE_KEYS[i].equals(languageKey)) {
        return true;
      }
    }
    return false;
  }

  /**
   * If the given file extension matches a declared .Net language, then this method returns the Sonar key of this language. 
   * If not, NULL is returned.
   */
  public static String getLanguageKeyFromFileExtension(String fileExtension) {
    return FILE_SUFFIXE_TO_LANGUAGE_KEY_MAP.get(fileExtension);
  }

}
