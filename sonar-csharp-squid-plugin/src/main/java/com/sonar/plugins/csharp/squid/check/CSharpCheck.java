/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.check;

import java.util.Collection;

import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;

import com.google.common.collect.Lists;
import com.sonar.csharp.squid.api.ast.CSharpAstCheck;

/**
 * Abstract class that every check developed for the C# language should extend to be automatically injected in the C# plugin.
 * 
 */
public class CSharpCheck extends CSharpAstCheck implements ServerExtension, BatchExtension {

  /**
   * Turns an array of {@link CSharpCheck} objects into a collection of their corresponding class.
   * 
   * @param checks
   * @return
   */
  @SuppressWarnings("rawtypes")
  public static Collection<Class> toCollection(CSharpCheck[] checks) {
    Collection<Class> checkClasses = Lists.newArrayList();
    for (int i = 0; i < checks.length; i++) {
      checkClasses.add(checks[i].getClass());
    }
    return checkClasses;
  }

}
