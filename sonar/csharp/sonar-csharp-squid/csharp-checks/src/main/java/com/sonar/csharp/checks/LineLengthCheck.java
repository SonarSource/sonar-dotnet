/*
 * Sonar C# Plugin :: C# Squid :: Checks
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
package com.sonar.csharp.checks;

import com.google.common.io.Files;
import com.sonar.csharp.squid.CharsetAwareVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.api.utils.SonarException;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;

import java.io.File;
import java.io.IOException;
import java.nio.charset.Charset;
import java.util.List;

@Rule(
  key = "LineLength",
  priority = Priority.MINOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MINOR)
public class LineLengthCheck extends SquidCheck<Grammar> implements CharsetAwareVisitor {

  private static final int DEFAULT_MAXIMUM_LINE_LENHGTH = 200;

  private Charset charset;

  @RuleProperty(
    key = "maximumLineLength",
    defaultValue = "" + DEFAULT_MAXIMUM_LINE_LENHGTH)
  public int maximumLineLength = DEFAULT_MAXIMUM_LINE_LENHGTH;

  public int getMaximumLineLength() {
    return maximumLineLength;
  }

  @Override
  public void visitFile(AstNode astNode) {
    File file = getContext().getFile();
    List<String> lines = readLines(file);
    for (int i = 0; i < lines.size(); i++) {
      int length = lines.get(i).length();
      if (length > getMaximumLineLength()) {
        getContext().createLineViolation(
            this,
            "Split this " + length + " characters long line (which is greater than " + getMaximumLineLength() + " authorized).",
            i + 1);
      }
    }
  }

  private List<String> readLines(File file) {
    try {
      return Files.readLines(file, charset);
    } catch (IOException e) {
      throw new SonarException("Unable to read " + file, e);
    }
  }

  public void setCharset(Charset charset) {
    this.charset = charset;
  }

}
