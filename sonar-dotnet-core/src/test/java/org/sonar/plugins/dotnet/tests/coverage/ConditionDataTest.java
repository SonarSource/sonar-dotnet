/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class ConditionDataTest {
  @Test
  public void givenConditionData_getUniqueKey_containsFilePathStartLineAndLocations(){
    assertThat(new ConditionData("opencover", "path", 1, new ConditionData.Location(2, 3), 4, 5, "coverageIdentifier").getUniqueKey()).isEqualTo("path-1-2-3-4");
  }

  @Test
  public void givenConditionData_getFormat_returnsFormat(){
    assertThat(new ConditionData("opencover", "path", 1, new ConditionData.Location(2, 3), 4, 5, "coverageIdentifier").getFormat()).isEqualTo("opencover");
    assertThat(new ConditionData("cobertura", "path", 1, new ConditionData.Location(2, 3), 4, 5, "coverageIdentifier").getFormat()).isEqualTo("cobertura");
    assertThat(new ConditionData("unmergeable", "path", 1, new ConditionData.Location(2, 3), 4, 5, "coverageIdentifier").getFormat()).isEqualTo("unmergeable");
  }
}
