/*
 * Sonar C# Plugin :: C# Squid :: Checks
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
package com.sonar.csharp.checks;

import com.google.common.collect.Lists;

import java.util.List;

public final class CheckList {

  public static final String SONAR_WAY_PROFILE = "Sonar way";

  private CheckList() {
  }

  public static List<Class> getChecks() {
    return Lists.<Class> newArrayList(
        AssignmentInsideSubExpressionCheck.class,
        AsyncAwaitIdentifierCheck.class,
        BreakOutsideSwitchCheck.class,
        ClassCouplingCheck.class,
        ClassNameCheck.class,
        CommentedCodeCheck.class,
        CommentRegularExpressionCheck.class,
        FileLocCheck.class,
        FunctionComplexityCheck.class,
        LineLengthCheck.class,
        MagicNumberCheck.class,
        MethodNameCheck.class,
        ParameterAssignedToCheck.class,
        ParsingErrorCheck.class,
        SwitchWithoutDefaultCheck.class,
        TabCharacterCheck.class,
        ForLoopCounterChangedCheck.class,
        XPathCheck.class);
  }

}
