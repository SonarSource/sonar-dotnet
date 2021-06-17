/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2021 SonarSource SA
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
package com.sonar.it.shared;

import org.junit.Rule;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;
import org.sonarqube.ws.Issues;

import static org.assertj.core.api.Assertions.assertThat;

public class WebConfigBase {
  static final String HOTSPOT_ERROR_MESSAGE = "Make sure disabling ASP.NET Request Validation feature is safe here.";
  static final String ISSUE_ERROR_MESSAGE = "Use a secure password when connecting to this database.";

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  protected void assertHotspot(Hotspot hotspot, int line, String fileName){
    assertThat(hotspot.getLine()).isEqualTo(line);
    assertThat(hotspot.getMessage()).isEqualTo(HOTSPOT_ERROR_MESSAGE);
    assertThat(hotspot.getComponent()).endsWith(fileName);
  }

  protected void assertIssue(Issues.Issue issue, int line, String fileName){
    assertThat(issue.getLine()).isEqualTo(line);
    assertThat(issue.getMessage()).isEqualTo(ISSUE_ERROR_MESSAGE);
    assertThat(issue.getComponent()).endsWith(fileName);
  }
}
