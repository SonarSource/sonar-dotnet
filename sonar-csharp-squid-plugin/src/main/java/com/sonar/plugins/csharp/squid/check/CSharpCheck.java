/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.check;

import com.google.common.collect.Lists;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;

import java.util.Collection;

/**
 * Abstract class that every check developed for the C# language should extend to be automatically injected in the C# plugin.
 *
 */
public class CSharpCheck extends SquidCheck<CSharpGrammar> implements ServerExtension, BatchExtension {

  /**
   * Turns an array of {@link CSharpCheck} objects into a collection of their corresponding class.
   *
   * @param checks
   * @return
   */
  @SuppressWarnings("rawtypes")
  public static Collection<Class> toCollection(CSharpCheck[] checks) {
    Collection<Class> checkClasses = Lists.newArrayList();
    for (CSharpCheck check : checks) {
      checkClasses.add(check.getClass());
    }
    return checkClasses;
  }

}
