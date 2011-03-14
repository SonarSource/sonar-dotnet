/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;

import java.util.List;
import java.io.File;
import java.net.MalformedURLException;

import org.junit.Before;
import org.junit.Test;
import org.sonar.plugin.dotnet.gendarme.model.Issue;
import org.sonar.plugin.dotnet.gendarme.stax.GendarmeResultStaxParser;

public class GendarmeResultParserTest {

  private static final String NAME = "AvoidRedundancyInMethodNameRule";
  private static final String PROBLEM = "This method's name includes the type name of the first parameter. This usually makes an API more verbose and less future-proof than necessary.";
  private static final String SOLUTION = "Remove the type from the method name, move the method into the parameter's type, or create an extension method (if using C#).";
  private static final String ASSEMBLY = "Example.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  private static final String LOCATION = "Example.Core.IMoney Example.Core.IMoney::AddMoney(Example.Core.Money)";
  private static final String SOURCE = "";

  private GendarmeResultStaxParser parserStax;

  @Before
  public void setUp() {    

    parserStax = new GendarmeResultStaxParser();

  }

  @Test
  public void testStaxParse() throws MalformedURLException {

    List<Issue> issues = parserStax.parse(new File("target/test-classes", "gendarme-report.xml"));
    Issue firstIssue = issues.get(0);
    Issue secondIssue = issues.get(1);

    // There are 33 issues in the gendarme-report.xml
    assertEquals(33, issues.size());

    assertNotNull( firstIssue );
    assertNotNull( secondIssue );

    // First and second issues should have the same Problem message and Solution message
    // But not the same Location
    assertEquals( NAME, firstIssue.getName() );
    assertEquals( PROBLEM, firstIssue.getProblem() );
    assertEquals( SOLUTION, firstIssue.getSolution() );
    assertEquals( ASSEMBLY, firstIssue.getAssembly() );
    assertEquals( LOCATION, firstIssue.getLocation() );
    assertEquals( SOURCE, firstIssue.getSource() );

    assertEquals( PROBLEM, secondIssue.getProblem() );
    assertEquals( SOLUTION, secondIssue.getSolution() );
    assertNotSame( LOCATION, secondIssue.getLocation() );

    for (Issue issue : issues) {
      assertNotNull( issue );
      assertNotNull( issue.getName() );
    }

  }
}
