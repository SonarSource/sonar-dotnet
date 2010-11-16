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
  private final boolean ignore_literals;
  private final Charset charset;

  public CSharpCPDMapping(CSharp csharp, Project project) {
    this.csharp = csharp;
    this.charset = project.getFileSystem().getSourceCharset();
    ignore_literals = project.getConfiguration().getBoolean(CSharpConstants.CPD_IGNORE_LITERALS_PROPERTY,
        CSharpConstants.CPD_IGNORE_LITERALS_DEFVALUE);
  }

  public Language getLanguage() {
    return csharp;
  }

  public Tokenizer getTokenizer() {
    return new CSharpCPDTokenizer(ignore_literals, charset);
  }

}
