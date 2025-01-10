/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2025 SonarSource SA
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
package org.sonarsource.vbnet.core;

class TestVbNetMetadata extends VbNetCorePluginMetadata {

  public static final TestVbNetMetadata INSTANCE = new TestVbNetMetadata();

  @Override
  public String pluginKey() {
    return "VbNet Test Plugin Key";
  }

  @Override
  public String analyzerProjectName() {
    return "VbNet Test Project Name";
  }

  @Override
  public String resourcesDirectory() {
    return "VbNet Test Resources Directory";
  }
}
