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

import org.junit.Test;

public class DefectLocationTest {

  @Test
  public void testParse() {
    Object[][] tests = {
        { "C:\\Workspace (Dev)\\Test.cs", null, "C:\\Workspace (Dev)\\Test.cs" },
        { "C:\\Workspace (Dev)\\Test.cs", 12, "C:\\Workspace (Dev)\\Test.cs(~12)" },
        { "C:\\Workspace (Dev)\\Test.cs", 12, "C:\\Workspace (Dev)\\Test.cs(12,15)" },
        { "Test.cs", 20, "Test.cs(~20)"},
        { "C:\\Build Env\\Test.cs", 20, "C:\\Build Env\\Test.cs(20,21)" }, };
   
    for (Object[] test : tests) {
      String source = (String) test[2];
      DefectLocation location = DefectLocation.parse(source);
      assertEquals((String) test[0], location.getPath());
      assertEquals((Integer) test[1], location.getLineNumber());
    }
  }

}
