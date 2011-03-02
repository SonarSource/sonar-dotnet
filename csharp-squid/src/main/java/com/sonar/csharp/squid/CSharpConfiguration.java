/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid;

import java.nio.charset.Charset;

import org.sonar.squid.api.SquidConfiguration;

public class CSharpConfiguration extends SquidConfiguration {

  public CSharpConfiguration(Charset charset) {
    super(charset);
  }

  public CSharpConfiguration() {
    super();
  }

}
