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
  static final String HOTSPOT_REQUEST_VALIDATION_ERROR_MESSAGE = "Make sure disabling ASP.NET Request Validation feature is safe here.";
  static final String HOTSPOT_CONTENT_LIMIT_ERROR_MESSAGE = "Make sure the content length limit is safe here.";
  static final String ISSUE_ERROR_MESSAGE = "Use a secure password when connecting to this database.";

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  protected void assertRequestValidationHotspot(Hotspot hotspot, int line, String fileName){
    assertHotspot(hotspot, line, fileName, HOTSPOT_REQUEST_VALIDATION_ERROR_MESSAGE);
  }

  protected void assertContentLengthHotspot(Hotspot hotspot, int line, String fileName){
    assertHotspot(hotspot, line, fileName, HOTSPOT_CONTENT_LIMIT_ERROR_MESSAGE);
  }

  protected void assertIssue(Issues.Issue issue, int line, String fileName){
    assertThat(issue.getLine()).isEqualTo(line);
    assertThat(issue.getMessage()).isEqualTo(ISSUE_ERROR_MESSAGE);
    assertThat(issue.getComponent()).endsWith(fileName);
  }

  private void assertHotspot(Hotspot hotspot, int line, String fileName, String errorMessage){
    assertThat(hotspot.getLine()).isEqualTo(line);
    assertThat(hotspot.getMessage()).isEqualTo(errorMessage);
    assertThat(hotspot.getComponent()).endsWith(fileName);
  }
}
