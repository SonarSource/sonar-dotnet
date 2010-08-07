package org.sonar.plugin.dotnet.coverage;

public abstract class PartCoverParsingStrategy extends AbstractParsingStrategy {

  public PartCoverParsingStrategy() {
    setPointElement("pt");
    setFileIdPointAttribute("fid");
    setCountVisitsPointAttribute("visit");
    setStartLinePointAttribute("sl");
    setEndLinePointAttribute("el");
  }

}
