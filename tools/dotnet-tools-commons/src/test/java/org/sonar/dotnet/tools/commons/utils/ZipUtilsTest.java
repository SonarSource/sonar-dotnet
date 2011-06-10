/*
 * .NET tools :: Commons
 * Copyright (C) 2011 Jose Chillan, Alexandre Victoor and SonarSource
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

package org.sonar.dotnet.tools.commons.utils;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;

import org.junit.Test;
import org.sonar.test.TestUtils;

public class ZipUtilsTest {

  @Test
  public void testExtractEmbeddedExecutable() throws Exception {
    String fakeJar = TestUtils.getResource("/zip-utils/Fake-Gendarme-JAR.jar").getAbsolutePath();
    File temp = new File("target/sonar/extract-jar");
    temp.mkdirs();

    File extractedFolder = ZipUtils.extractArchiveFolderIntoDirectory(fakeJar, "gendarme-2.10-bin", temp.getAbsolutePath());
    assertTrue(new File(extractedFolder, "gendarme.exe").exists());
    assertFalse(new File(temp, "OtherFile.txt").exists());
  }
}
