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

import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;

@Rule(
  key = "FileLoc",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class FileLocCheck extends SquidCheck<Grammar> implements AstAndTokenVisitor {

  private static final int DEFAULT_MAXIMUM_FILE_LOC_THRESHOLD = 1000;

  @RuleProperty(
    key = "maximumFileLocThreshold",
    defaultValue = "" + DEFAULT_MAXIMUM_FILE_LOC_THRESHOLD)
  public int maximumFileLocThreshold = DEFAULT_MAXIMUM_FILE_LOC_THRESHOLD;

  private int numberOfLoc = 0;
  private int lastTokenLine = -1;

  @Override
  public void visitFile(AstNode node) {
    numberOfLoc = 0;
    lastTokenLine = -1;
  }

  @Override
  public void leaveFile(AstNode node) {
    if (numberOfLoc > maximumFileLocThreshold) {
      getContext().createFileViolation(
          this,
          "This file has " + numberOfLoc + " lines of code, which is greater than " + maximumFileLocThreshold + " authorized. Split it into smaller files.");
    }
  }

  public void visitToken(Token token) {
    if (lastTokenLine != token.getLine()) {
      lastTokenLine = token.getLine();
      numberOfLoc++;
    }
  }

}
