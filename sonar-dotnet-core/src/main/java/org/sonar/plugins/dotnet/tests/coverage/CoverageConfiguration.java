/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonar.plugins.dotnet.tests.coverage;

public class CoverageConfiguration {

  private final String languageKey;
  private final String ncover3PropertyKey;
  private final String openCoverPropertyKey;
  private final String dotCoverPropertyKey;
  private final String visualStudioCoverageXmlPropertyKey;

  public CoverageConfiguration(String languageKey, String ncover3PropertyKey, String openCoverPropertyKey, String dotCoverPropertyKey, String visualStudioCoverageXmlPropertyKey) {
    this.languageKey = languageKey;
    this.ncover3PropertyKey = ncover3PropertyKey;
    this.openCoverPropertyKey = openCoverPropertyKey;
    this.dotCoverPropertyKey = dotCoverPropertyKey;
    this.visualStudioCoverageXmlPropertyKey = visualStudioCoverageXmlPropertyKey;
  }

  public String languageKey() {
    return languageKey;
  }

  public String ncover3PropertyKey() {
    return ncover3PropertyKey;
  }

  public String openCoverPropertyKey() {
    return openCoverPropertyKey;
  }

  public String dotCoverPropertyKey() {
    return dotCoverPropertyKey;
  }

  public String visualStudioCoverageXmlPropertyKey() {
    return visualStudioCoverageXmlPropertyKey;
  }

}
