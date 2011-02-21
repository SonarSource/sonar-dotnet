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

package org.sonar.plugin.dotnet.srcmon;

import static org.junit.Assert.*;

import java.io.File;
import java.util.regex.Pattern;

import org.junit.Ignore;
import org.junit.Test;

public class BlankLineCounterTest {

  @Test
  public void trickyTestCountBlankLines() {
    File csFile 
      = new File("target/test-classes/IIssueModificationService.cs");
    int nbBlanks = BlankLineCounter.countBlankLines(csFile);
    assertEquals(2, nbBlanks);
  }
  
  @Test
  public void monoTestCountBlankLines() {
    File csFile 
      = new File("target/test-classes/Helpers.cs");
    int nbBlanks = BlankLineCounter.countBlankLines(csFile);
    assertEquals(4, nbBlanks);
  }

}
