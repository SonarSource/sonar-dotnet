/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import javax.annotation.CheckForNull;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.TextRange;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

public final class SensorContextUtils {
  private SensorContextUtils() {
    // utility class, forbidden constructor
  }

  @CheckForNull
  public static InputFile toInputFile(FileSystem fs, String file) {
    return fs.inputFile(fs.predicates().hasPath(file));
  }

  public static TextRange toTextRange(InputFile inputFile, SonarAnalyzer.TextRange pbTextRange) {
    int startLine = pbTextRange.getStartLine();
    int startLineOffset = pbTextRange.getStartOffset();
    int endLine = pbTextRange.getEndLine();
    int endLineOffset = pbTextRange.getEndOffset();
    return inputFile.newRange(startLine, startLineOffset, endLine, endLineOffset);
  }
}
