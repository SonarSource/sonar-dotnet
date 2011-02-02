/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.cpd;

import java.nio.charset.Charset;

import net.sourceforge.pmd.cpd.Tokenizer;

import org.sonar.api.batch.AbstractCpdMapping;
import org.sonar.api.resources.Language;
import org.sonar.api.resources.Project;

import com.sonar.csharp.CSharp;
import com.sonar.csharp.CSharpConstants;

public class CSharpCPDMapping extends AbstractCpdMapping {

  private final CSharp csharp;
  private final boolean ignoreLiterals;
  private final Charset charset;

  public CSharpCPDMapping(CSharp csharp, Project project) {
    this.csharp = csharp;
    this.charset = project.getFileSystem().getSourceCharset();
    ignoreLiterals = project.getConfiguration().getBoolean(CSharpConstants.CPD_IGNORE_LITERALS_PROPERTY,
        CSharpConstants.CPD_IGNORE_LITERALS_DEFVALUE);
  }

  public Language getLanguage() {
    return csharp;
  }

  public Tokenizer getTokenizer() {
    return new CSharpCPDTokenizer(ignoreLiterals, charset);
  }

}
