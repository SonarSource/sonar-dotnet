/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.csharp.core;

import java.util.Objects;
import org.sonar.api.config.Configuration;
import org.sonar.api.resources.AbstractLanguage;
import org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public abstract class CSharpCorePluginMetadata implements PluginMetadata {

  @Override
  public String languageKey() {
    return CSharp.LANGUAGE_KEY;
  }

  @Override
  public String languageName() {
    return CSharp.LANGUAGE_NAME;
  }

  @Override
  public String repositoryKey() {
    return "csharpsquid";
  }

  @Override
  public String fileSuffixesKey() {
    return AbstractPropertyDefinitions.fileSuffixProperty(languageKey());
  }

  @Override
  public String fileSuffixesDefaultValue() {
    return ".cs,.razor";
  }

  public class CSharp extends AbstractLanguage {
    // Do not make these fields public and access them directly. Use the methods in CSharpCorePluginMetadata instead
    private static final String LANGUAGE_KEY = "cs";
    private static final String LANGUAGE_NAME = "C#";

    private final Configuration configuration;

    public CSharp(Configuration configuration) {
      super(languageKey(), languageName());
      this.configuration = configuration;
    }

    @Override
    public String[] getFileSuffixes() {
      return configuration.getStringArray(fileSuffixesKey());
    }

    @Override
    public boolean equals(Object o) {
      return super.equals(o) && o instanceof CSharp cSharp && configuration == cSharp.configuration;
    }

    @Override
    public int hashCode() {
      return Objects.hash(super.hashCode(), configuration.hashCode());
    }
  }
}
