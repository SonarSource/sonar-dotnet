/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid;

import java.nio.charset.Charset;

import org.sonar.squid.api.SquidConfiguration;

public class CSharpConfiguration extends SquidConfiguration {

  private boolean ignoreHeaderComments = true;

  public CSharpConfiguration(Charset charset) {
    super(charset);
  }

  public CSharpConfiguration() {
    super();
  }

  public void setIgnoreHeaderComments(boolean ignoreHeaderComments) {
    this.ignoreHeaderComments = ignoreHeaderComments;
  }

  public boolean getIgnoreHeaderComments() {
    return ignoreHeaderComments;
  }

}
