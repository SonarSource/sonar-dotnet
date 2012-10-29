/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp.squid.check;

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
