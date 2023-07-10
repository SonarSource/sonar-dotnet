/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2023 SonarSource SA
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
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import java.io.IOException;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getIssues;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class CasingAppTest {

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() {
    TestUtils.initLocal(ORCHESTRATOR);
  }

  @Test
  public void class1_should_have_metrics_and_issues() throws IOException {
    String projectKey = "CasingApp";
    String componentKey = "CasingApp:CasingApp/SRC/Class1.cs";

    Tests.analyzeProject(temp, projectKey, null);

    assertThat(getComponent(componentKey)).isNotNull();
    assertThat(getMeasureAsInt(componentKey, "files")).isEqualTo(1);
    assertThat(getMeasureAsInt(componentKey, "lines")).isEqualTo(10);
    assertThat(getMeasureAsInt(componentKey, "ncloc")).isEqualTo(9);
    assertThat(getIssues(componentKey)).hasSize(1);
  }
}
