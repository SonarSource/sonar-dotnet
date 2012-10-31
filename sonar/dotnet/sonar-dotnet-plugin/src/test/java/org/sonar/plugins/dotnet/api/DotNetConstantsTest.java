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
package org.sonar.plugins.dotnet.api;

import org.junit.Test;

import static org.fest.assertions.Assertions.assertThat;

public class DotNetConstantsTest {

  @Test
  public void shouldReturnPropKeysForDotNetSdkAndSilverlightDirs() {
    assertThat(DotNetConstants.getDotnetSdkDirKey("2.0")).isEqualTo(DotNetConstants.DOTNET_2_0_SDK_DIR_KEY);
    assertThat(DotNetConstants.getDotnetSdkDirKey("3.5")).isEqualTo(DotNetConstants.DOTNET_3_5_SDK_DIR_KEY);
    assertThat(DotNetConstants.getDotnetSdkDirKey("4.0")).isEqualTo(DotNetConstants.DOTNET_4_0_SDK_DIR_KEY);

    assertThat(DotNetConstants.getSilverlightDirKey("3")).isEqualTo(DotNetConstants.SILVERLIGHT_3_MSCORLIB_LOCATION_KEY);
    assertThat(DotNetConstants.getSilverlightDirKey("4")).isEqualTo(DotNetConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_KEY);
  }

}
