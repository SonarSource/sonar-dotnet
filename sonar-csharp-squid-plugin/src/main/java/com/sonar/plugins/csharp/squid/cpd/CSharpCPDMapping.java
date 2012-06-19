/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.cpd;

import java.nio.charset.Charset;

import net.sourceforge.pmd.cpd.Tokenizer;

import org.sonar.api.batch.AbstractCpdMapping;
import org.sonar.api.resources.Language;
import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.api.CSharp;

import com.sonar.plugins.csharp.squid.CSharpSquidConstants;

public class CSharpCPDMapping extends AbstractCpdMapping {

  private final CSharp csharp;
  private final boolean ignoreLiterals;
  private final Charset charset;

  public CSharpCPDMapping(CSharp csharp, Project project) {
    super();
    this.csharp = csharp;
    this.charset = project.getFileSystem().getSourceCharset();
    ignoreLiterals = project.getConfiguration().getBoolean(CSharpSquidConstants.CPD_IGNORE_LITERALS_PROPERTY,
        CSharpSquidConstants.CPD_IGNORE_LITERALS_DEFVALUE);
  }

  public Language getLanguage() {
    return csharp;
  }

  public Tokenizer getTokenizer() {
    return new CSharpCPDTokenizer(ignoreLiterals, charset);
  }

}
