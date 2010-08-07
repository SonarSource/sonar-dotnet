package org.sonar.plugin.dotnet.core.colorizer;

import org.sonar.colorizer.InlineDocTokenizer;

public class XmlDocTokenizer extends InlineDocTokenizer {

  public XmlDocTokenizer(String tagBefore, String tagAfter) {
    super("///", tagBefore, tagAfter);
  }

}