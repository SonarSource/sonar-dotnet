/*
 * .NET tools :: Commons
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
package org.sonar.dotnet.tools.commons.utils;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.io.FileInputStream;

import org.apache.commons.codec.digest.DigestUtils;
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

    // check also MD5 checksum on a 2nd file, to be sure the extraction worked properly
    File readmeFile = new File(extractedFolder, "README.txt");
    assertTrue(readmeFile.exists());
    String readmeFileMD5 = DigestUtils.md5Hex(new FileInputStream(readmeFile));
    assertThat(readmeFileMD5, is("01de713b0acdcfd7db5a138a910696a5"));

    // and check the following file does not exist (whereas it is in the JAR, but not in the "gendarme-2.10-bin" folder
    assertFalse(new File(temp, "OtherFile.txt").exists());
  }

}
