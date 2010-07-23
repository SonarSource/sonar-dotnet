package org.sonar.plugin.dotnet.partcover;

public class PartCover23ParsingStrategy extends PartCover2ParsingStrategy {

	public PartCover23ParsingStrategy() {
		setFilePath("/*/File");
    setMethodPath("/*/Type/Method");
    setPartcoverExactVersion("2.3");
  }

}
