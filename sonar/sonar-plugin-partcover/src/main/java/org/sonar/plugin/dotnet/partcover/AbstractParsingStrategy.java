package org.sonar.plugin.dotnet.partcover;

import org.w3c.dom.Element;

public abstract class AbstractParsingStrategy {

	private String filePath;
	private String methodPath;

	public String getFilePath() {
  	return filePath;
  }

	public void setFilePath(String filePath) {
  	this.filePath = filePath;
  }

	public String getMethodPath() {
  	return methodPath;
  }

	public void setMethodPath(String methodPath) {
  	this.methodPath = methodPath;
  }
  
  abstract String findAssemblyName(Element methodElement); 
  
  abstract boolean isCompatible(Element rootElement);
	
}
