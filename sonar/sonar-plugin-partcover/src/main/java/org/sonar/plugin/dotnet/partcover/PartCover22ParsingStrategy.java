package org.sonar.plugin.dotnet.partcover;

public class PartCover22ParsingStrategy extends PartCover2ParsingStrategy {

	public PartCover22ParsingStrategy() {
		setFilePath("/*/file");
    setMethodPath("/*/type/method");
    setPartcoverExactVersion("2.2");
  }
}
