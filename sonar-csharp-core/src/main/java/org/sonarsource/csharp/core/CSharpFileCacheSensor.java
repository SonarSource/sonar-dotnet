/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

import org.sonarsource.dotnet.shared.plugins.HashProvider;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.sensors.AbstractFileCacheSensor;

public class CSharpFileCacheSensor extends AbstractFileCacheSensor {
  public CSharpFileCacheSensor(PluginMetadata metadata, HashProvider hashProvider) {
    super(metadata, hashProvider);
  }

  @Override
  protected String[] additionalSupportedExtensions() {
    return new String[]{"cshtml"};
  }
}
