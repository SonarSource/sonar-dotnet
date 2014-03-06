/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
package com.sonar.csharp.squid;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.AstScanner;
import com.sonar.sslr.squid.SquidAstVisitor;
import com.sonar.sslr.squid.SquidAstVisitorContextImpl;

import javax.annotation.Nullable;

import java.io.File;
import java.util.Collection;
import java.util.concurrent.TimeUnit;

public class ProgressAstScanner extends AstScanner<Grammar> {

  private final ProgressReport progressReport;

  protected ProgressAstScanner(Builder builder) {
    super(builder);
    this.progressReport = builder.progressReport;
  }

  @Override
  public void scanFiles(Collection<File> files) {
    progressReport.start(files.size());
    super.scanFiles(files);
    progressReport.stop();
  }

  public static class Builder extends AstScanner.Builder<Grammar> {

    private final ProgressReport progressReport = new ProgressReport("Report about progress of C# analyzer", TimeUnit.SECONDS.toMillis(10));

    public Builder(SquidAstVisitorContextImpl<Grammar> context) {
      super(context);
    }

    @Override
    public AstScanner<Grammar> build() {
      super.withSquidAstVisitor(new SquidAstVisitor<Grammar>() {

        @Override
        public void visitFile(@Nullable AstNode astNode) {
          progressReport.nextFile(getContext().getFile());
        }

      });

      return new ProgressAstScanner(this);
    }

  }

}
