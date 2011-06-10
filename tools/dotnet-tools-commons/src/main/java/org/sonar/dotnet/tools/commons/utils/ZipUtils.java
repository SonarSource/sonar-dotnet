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

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Enumeration;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

import org.apache.commons.io.IOUtils;

/**
 * ZIP Utilities
 */
public final class ZipUtils {

  private static final int BUFFER_SIZE = 2048;

  private ZipUtils() {
  }

  /**
   * Extracts the specified folder from the specified archive, into the supplied output directory.
   * 
   * @param archivePath
   *          the archive Path
   * @param folderToExtract
   *          the folder to extract
   * @param outputDirectory
   *          the output directory
   * @return the extracted folder path
   * @throws IOException
   *           if a problem occurs while extracting
   */
  public static File extractArchiveFolderIntoDirectory(String archivePath, String folderToExtract, String outputDirectory)
      throws IOException {
    File destinationFolder = new File(outputDirectory);
    destinationFolder.mkdirs();

    BufferedInputStream is = null;
    BufferedOutputStream dest = null;
    try {
      ZipFile zip = new ZipFile(new File(archivePath));
      Enumeration<?> zipFileEntries = zip.entries();
      // Process each entry
      while (zipFileEntries.hasMoreElements()) {
        ZipEntry entry = (ZipEntry) zipFileEntries.nextElement();
        String currentEntry = entry.getName();
        if (currentEntry.startsWith(folderToExtract)) {
          File destFile = new File(destinationFolder, currentEntry);
          destFile.getParentFile().mkdirs();
          if ( !entry.isDirectory()) {
            is = new BufferedInputStream(zip.getInputStream(entry));
            int currentByte;
            // establish buffer for writing file
            byte data[] = new byte[BUFFER_SIZE];

            // write the current file to disk
            FileOutputStream fos = new FileOutputStream(destFile);
            dest = new BufferedOutputStream(fos, BUFFER_SIZE);

            // read and write until last byte is encountered
            while ((currentByte = is.read(data, 0, BUFFER_SIZE)) != -1) {
              dest.write(data, 0, currentByte);
            }
          }
        }
      }
    } finally {
      if (dest != null) {
        dest.flush();
      }
      IOUtils.closeQuietly(dest);
      IOUtils.closeQuietly(is);
    }
    return new File(destinationFolder, folderToExtract);
  }

}
