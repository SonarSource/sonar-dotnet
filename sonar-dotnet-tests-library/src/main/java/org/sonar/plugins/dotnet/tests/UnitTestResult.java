package org.sonar.plugins.dotnet.tests;

public class UnitTestResult {

  public enum Status {
    PASSED,
    FAILED,
    SKIPPED
  }

  private Long durationInMs;
  private Status status;

  private String fullyQualifiedName;

  public UnitTestResult(Long durationInMs, Status status, String fullyQualifiedName) {

    this.durationInMs = durationInMs;
    this.status = status;
    this.fullyQualifiedName = fullyQualifiedName;
  }

  public Long getDurationInMs() {
    return durationInMs;
  }

  public Status getStatus() {
    return status;
  }

  public String getFullyQualifiedName() {
    return fullyQualifiedName;
  }

  public void setFullyQualifiedName(String fullyQualifiedName) {
    this.fullyQualifiedName = fullyQualifiedName;
  }
}
