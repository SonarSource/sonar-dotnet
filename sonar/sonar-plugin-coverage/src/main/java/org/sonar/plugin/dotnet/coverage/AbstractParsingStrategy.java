package org.sonar.plugin.dotnet.coverage;

import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.w3c.dom.Element;

public abstract class AbstractParsingStrategy extends AbstractXmlParser {

  private String filePath;
  private String methodPath;

  private String pointElement;
  private String countVisitsPointAttribute;
  private String startLinePointAttribute;
  private String endLinePointAttribute;
  private String fileIdPointAttribute;

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

  public String getCountVisitsPointAttribute() {
    return countVisitsPointAttribute;
  }

  public void setCountVisitsPointAttribute(String countVisitsPointAttribute) {
    this.countVisitsPointAttribute = countVisitsPointAttribute;
  }

  public String getStartLinePointAttribute() {
    return startLinePointAttribute;
  }

  public void setStartLinePointAttribute(String startLinePointAttribute) {
    this.startLinePointAttribute = startLinePointAttribute;
  }

  public String getEndLinePointAttribute() {
    return endLinePointAttribute;
  }

  public void setEndLinePointAttribute(String endLinePointAttribute) {
    this.endLinePointAttribute = endLinePointAttribute;
  }

  public String getPointElement() {
    return pointElement;
  }

  public void setPointElement(String pointElement) {
    this.pointElement = pointElement;
  }

  public String getFileIdPointAttribute() {
    return fileIdPointAttribute;
  }

  public void setFileIdPointAttribute(String fileIdPointAttribute) {
    this.fileIdPointAttribute = fileIdPointAttribute;
  }

}
