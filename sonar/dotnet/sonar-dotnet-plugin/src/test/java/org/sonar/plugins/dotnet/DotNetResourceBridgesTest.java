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
package org.sonar.plugins.dotnet;

import org.junit.Test;
import org.sonar.plugins.dotnet.api.DotNetResourceBridge;
import org.sonar.plugins.dotnet.api.DotNetResourceBridges;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class DotNetResourceBridgesTest {

  @Test
  public void shouldFindBridges() {
    DotNetResourceBridge bridge1 = mock(DotNetResourceBridge.class);
    when(bridge1.getLanguageKey()).thenReturn("cs");
    DotNetResourceBridge bridge2 = mock(DotNetResourceBridge.class);
    when(bridge2.getLanguageKey()).thenReturn("vbnet");

    DotNetResourceBridges b = new DotNetResourceBridges(new DotNetResourceBridge[] {bridge1, bridge2});
    assertThat(b.getBridge("cs")).isEqualTo(bridge1);
    assertThat(b.getBridge("vbnet")).isEqualTo(bridge2);
    assertThat(b.getBridge("java")).isNull();
  }

  @Test
  public void shouldNotFindBridgesIfNoDotNetPlugin() {
    DotNetResourceBridges b = new DotNetResourceBridges();
    assertThat(b.getBridge("cs")).isNull();
  }

}
