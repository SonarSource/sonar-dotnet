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
    keywords.addAll(Arrays.asList(new String[] { "abstract", "add", "as",
        "ascending", "base", "bool", "break", "by", "byte", "case", "catch",
        "char", "checked", "class", "const", "continue", "decimal", "default",
        "delegate", "do", "double", "descending", "else", "enum", "event",
        "explicit", "extern", "false", "finally", "fixed", "from", "float",
        "for", "foreach", "get", "group", "goto", "if", "implicit", "in",
        "int", "into", "interface", "internal", "is", "lock", "long", "join",
        "let", "namespace", "new", "null", "object", "operator", "orderby",
        "out", "override", "params", "partial", "private", "protected",
        "public", "readonly", "ref", "remove", "return", "sbyte", "sealed",
        "select", "set", "short", "sizeof", "stackalloc", "static", "string",
        "struct", "switch", "this", "throw", "true", "try", "typeof", "uint",
        "ulong", "unchecked", "unsafe", "ushort", "using", "value", "virtual",
        "void", "volatile", "where", "while", "yield" }));
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
