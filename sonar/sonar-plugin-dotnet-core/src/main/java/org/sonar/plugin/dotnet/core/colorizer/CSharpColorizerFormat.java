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

package org.sonar.plugin.dotnet.core.colorizer;

import java.util.Arrays;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.sonar.api.web.CodeColorizerFormat;
import org.sonar.colorizer.CDocTokenizer;
import org.sonar.colorizer.CppDocTokenizer;
import org.sonar.colorizer.KeywordsTokenizer;
import org.sonar.colorizer.StringTokenizer;
import org.sonar.colorizer.Tokenizer;
import org.sonar.plugin.dotnet.core.CSharp;

public class CSharpColorizerFormat extends CodeColorizerFormat {

  private static Set<String> keywords;

  static {
    keywords = new HashSet<String>();
    keywords.addAll(Arrays.asList(new String[] { "abstract", "add", "alias",
        "as", "ascending", "base", "bool", "break", "by", "byte", "case",
        "catch", "char", "checked", "class", "const", "continue", "decimal",
        "default", "delegate", "descending", "do", "double", "dynamic", "else",
        "enum", "equals", "event", "explicit", "extern", "false", "finally",
        "fixed", "float", "for", "foreach", "from", "get", "global", "goto",
        "group", "if", "implicit", "in", "int", "interface", "internal",
        "into", "is", "join", "let", "lock", "long", "namespace", "new",
        "null", "object", "on", "operator", "orderby", "out", "override",
        "params", "partial", "private", "protected", "public", "readonly",
        "ref", "remove", "return", "sbyte", "sealed", "select", "set", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
        "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "value", "var", "virtual", "void",
        "volatile", "where", "while", "yield" }));
  }

  public CSharpColorizerFormat() {
    super(CSharp.KEY);
  }

  @Override
  public List<Tokenizer> getTokenizers() {
    return Collections.unmodifiableList(Arrays.asList(new StringTokenizer(
        "<span class=\"s\">", "</span>"), new XmlDocTokenizer(
        "<span class=\"cppd\">", "</span>"), new CDocTokenizer(
        "<span class=\"cd\">", "</span>"), new CppDocTokenizer(
        "<span class=\"cppd\">", "</span>"), new KeywordsTokenizer(
        "<span class=\"k\">", "</span>", keywords)));
  }

}
