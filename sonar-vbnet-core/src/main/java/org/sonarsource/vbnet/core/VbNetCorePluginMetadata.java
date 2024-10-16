/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.vbnet.core;

import java.util.Objects;
import org.sonar.api.config.Configuration;
import org.sonar.api.resources.AbstractLanguage;
import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public abstract class VbNetCorePluginMetadata implements PluginMetadata {

  @Override
  public String languageKey() {
    return VbNet.LANGUAGE_KEY;
  }

  @Override
  public String languageName() {
    return VbNet.LANGUAGE_NAME;
  }

  @Override
  public String repositoryKey() {
    return "vbnet";
  }

  @Override
  public String fileSuffixesKey() {
    return AbstractPropertyDefinitions.getFileSuffixProperty(languageKey());
  }

  @Override
  public String fileSuffixesDefaultValue() {
    return ".vb";
  }

  @Override
  public String resourcesDirectory() {
    return "/org/sonar/plugins/vbnet";
  }

  public class VbNet extends AbstractLanguage {

    // Do not make these fields public and access them directly. Use the methods in VBnetCorePluginMetadata instead.
    private static final String LANGUAGE_KEY = "vbnet";
    private static final String LANGUAGE_NAME = "VB.NET";
    private final Configuration configuration;

    public VbNet(Configuration configuration) {
      super(languageKey(), languageName());
      this.configuration = configuration;
    }

    @Override
    public String[] getFileSuffixes() {
      return configuration.getStringArray(fileSuffixesKey());
    }

    @Override
    public boolean equals(Object o) {
      return super.equals(o) && o instanceof VbNet vbNet && configuration == vbNet.configuration;
    }

    @Override
    public int hashCode() {
      return Objects.hash(super.hashCode(), configuration.hashCode());
    }
  }
}
