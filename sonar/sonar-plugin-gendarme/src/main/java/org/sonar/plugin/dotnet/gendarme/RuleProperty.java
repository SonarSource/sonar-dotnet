package org.sonar.plugin.dotnet.gendarme;

public class RuleProperty {

  private final String name;
  
  private final String value;
  
  public RuleProperty(String name, String value) {
    this.name = name;
    this.value = value;
  }

  public String getName() {
    return name;
  }

  public String getValue() {
    return value;
  }
  
}
