/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp;

import com.google.common.base.Throwables;
import com.google.common.io.ByteStreams;
import com.google.common.io.Files;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.util.Enumeration;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

public class Zip {

  private final File zipFile;

  public Zip(File zipFile) {
    this.zipFile = zipFile;
  }

  public void unzip(File out) {
    try {
      ZipFile zip = new ZipFile(zipFile);
      try {
        Enumeration<? extends ZipEntry> entries = zip.entries();

        while (entries.hasMoreElements()) {
          ZipEntry entry = entries.nextElement();

          if (!entry.isDirectory()) {
            InputStream is = zip.getInputStream(entry);
            try {
              File outFile = new File(out, entry.getName());
              Files.createParentDirs(outFile);
              Files.write(ByteStreams.toByteArray(is), outFile);
            } finally {
              is.close();
            }
          } else {
            File fakeFileInOutFolder = new File(new File(out, entry.getName()), "fake.txt");
            Files.createParentDirs(fakeFileInOutFolder);
          }
        }
      } finally {
        zip.close();
      }
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }
  }

}
