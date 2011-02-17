/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin;

import org.apache.commons.configuration.Configuration;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.resources.AbstractLanguage;

public class CSharp extends AbstractLanguage {

  private Configuration configuration;

  public CSharp(Configuration configuration) {
    super(CSharpConstants.LANGUAGE_KEY, CSharpConstants.LANGUAGE_NAME);
    this.configuration = configuration;
  }

  public String[] getFileSuffixes() {
    String[] suffixes = configuration.getStringArray(CSharpConstants.FILE_SUFFIXES_KEY);
    if (suffixes == null || suffixes.length == 0) {
      suffixes = StringUtils.split(CSharpConstants.FILE_SUFFIXES_DEFVALUE, ",");
    }
    return suffixes;
  }
}
