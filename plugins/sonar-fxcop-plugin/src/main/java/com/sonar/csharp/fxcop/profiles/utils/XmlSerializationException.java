/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.profiles.utils;

/**
 * An exception for xml serialization errors.
 */
public class XmlSerializationException extends RuntimeException {

  /**
   * serialVersionUID
   */
  private static final long serialVersionUID = -544645686195710683L;

  /**
   * Constructs a @link{XmlSerializationException}.
   */
  public XmlSerializationException() {
  }

  /**
   * Constructs a @link{XmlSerializationException}.
   * 
   * @param message
   */
  public XmlSerializationException(String message) {
    super(message);

  }

  /**
   * Constructs a @link{XmlSerializationException}.
   * 
   * @param cause
   */
  public XmlSerializationException(Throwable cause) {
    super(cause);

  }

  /**
   * Constructs a @link{XmlSerializationException}.
   * 
   * @param message
   * @param cause
   */
  public XmlSerializationException(String message, Throwable cause) {
    super(message, cause);

  }

}
