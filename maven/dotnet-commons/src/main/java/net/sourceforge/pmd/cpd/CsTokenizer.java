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

package net.sourceforge.pmd.cpd;

import java.util.ArrayList;
import java.util.List;

/**
 * Code of the AbstractTokenizer adapted to the C# language. It was impossible
 * to extend AbstractTokenizer, some private methods could not be overridden.
 * TODO clean up
 * 
 * @author Alexandre Victoor
 */
public class CsTokenizer implements Tokenizer {

  private List<String> stringToken; // List<String>, should be setted by
                                    // children classes
  private List<String> ignorableCharacter; // List<String>, should be setted by
                                           // children classes
  // FIXME:Maybe an array of 'char' would be better for perfomance ?
  private List<String> ignorableStmt; // List<String>, should be setted by
                                      // children classes

  private List<String> code;
  private int lineNumber = 0;
  private String currentLine;

  protected boolean spanMultipleLinesString = true; // Most language does, so
                                                    // default is true

  private boolean downcaseString = true;

  private boolean lineComment = false;
  private boolean multipleLineComment = false;
  private boolean importLine = false;
  private boolean namespaceRead = false;

  public CsTokenizer() {

    // setting markers for "string" in csharp
    this.stringToken = new ArrayList<String>();
    this.stringToken.add("\"");
    // setting markers for 'ignorable character' in csharp
    this.ignorableCharacter = new ArrayList<String>();
    this.ignorableCharacter.add("{");
    this.ignorableCharacter.add("}");
    this.ignorableCharacter.add("(");
    this.ignorableCharacter.add(")");
    this.ignorableCharacter.add(";");
    this.ignorableCharacter.add(",");

    // setting markers for 'ignorable string' in csharp
    this.ignorableStmt = new ArrayList<String>();
    this.ignorableStmt.add("using");
    this.ignorableStmt.add("while");
    this.ignorableStmt.add("do");
    this.ignorableStmt.add("end");

  }

  public void tokenize(SourceCode tokens, Tokens tokenEntries) {
    this.code = tokens.getCode();

    for (this.lineNumber = 0; lineNumber < this.code.size(); lineNumber++) {
      this.currentLine = this.code.get(this.lineNumber);
      int loc = 0;
      lineComment = false;
      importLine = false;
      while (loc < currentLine.length()) {
        StringBuffer token = new StringBuffer();
        loc = getTokenFromLine(token, loc);

        if (lineComment || multipleLineComment || importLine) {
          if (token.toString().endsWith("*/")) {
            multipleLineComment = false;
            lineComment = false;
          }
        } else if (token.toString().startsWith("//")) {
          lineComment = true;
        } else if (token.toString().startsWith("/*")) {
          multipleLineComment = true;
        } else if (!namespaceRead && "using".equals(token.toString())) {
          importLine = true;
        } else if (token.length() > 0 && !isIgnorableString(token.toString())) {
          if ("namespace".equals(token.toString())) {
            namespaceRead = true;
          }

          if (downcaseString) {
            token = new StringBuffer(token.toString().toLowerCase());
          }
          if (CPD.debugEnable)
            System.out.println("Token added:" + token.toString());
          tokenEntries.add(new TokenEntry(token.toString(), tokens
              .getFileName(), lineNumber));

        }
      }
    }
    tokenEntries.add(TokenEntry.getEOF());
  }

  private int getTokenFromLine(StringBuffer token, int loc) {
    for (int j = loc; j < this.currentLine.length(); j++) {
      char tok = this.currentLine.charAt(j);
      if (!Character.isWhitespace(tok) && !ignoreCharacter(tok)) {
        if (isString(tok)) {
          if (token.length() > 0) {
            return j; // we need to now parse the string as a seperate token.
          } else {
            // we are at the start of a string
            return parseString(token, j, tok);
          }
        } else {
          token.append(tok);
        }
      } else {
        if (token.length() > 0) {
          return j;
        }
      }
      loc = j;
    }
    return loc + 1;
  }

  private int parseString(StringBuffer token, int loc, char stringDelimiter) {
    boolean escaped = false;
    boolean done = false;
    char tok = ' '; // this will be replaced.
    while ((loc < currentLine.length()) && !done) {
      tok = currentLine.charAt(loc);
      if (escaped && tok == stringDelimiter) // Found an escaped string
        escaped = false;
      else if (tok == stringDelimiter && (token.length() > 0)) // We are done,
                                                               // we found the
                                                               // end of the
                                                               // string...
        done = true;
      else if (tok == '\\') // Found an escaped char
        escaped = true;
      else
        // Adding char...
        escaped = false;
      // Adding char to String:" + token.toString());
      token.append(tok);
      loc++;
    }
    // Handling multiple lines string
    if (!done && // ... we didn't find the end of the string
        loc >= currentLine.length() && // ... we have reach the end of the line
                                       // ( the String is incomplete, for the
                                       // moment at least)
        this.spanMultipleLinesString && // ... the language allow multiple line
                                        // span Strings
        ++this.lineNumber < this.code.size() // ... there is still more lines to
                                             // parse
    ) {
      // parsing new line
      this.currentLine = this.code.get(this.lineNumber);
      // Warning : recursive call !
      loc = this.parseString(token, loc, stringDelimiter);
    }
    return loc + 1;
  }

  private boolean ignoreCharacter(char tok) {
    return this.ignorableCharacter.contains("" + tok);
  }

  private boolean isString(char tok) {
    return this.stringToken.contains("" + tok);
  }

  private boolean isIgnorableString(String token) {
    return this.ignorableStmt.contains(token);
  }

}
