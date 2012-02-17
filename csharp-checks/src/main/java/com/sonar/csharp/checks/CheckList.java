/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.checks;

import com.google.common.collect.Lists;

import java.util.List;

public final class CheckList {

  private CheckList() {
  }

  public static List<Class> getChecks() {
    return Lists.<Class> newArrayList(
        // CommentedCodeCheck.class,
        ParsingErrorCheck.class
        );
  }

}
