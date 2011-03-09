/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.runner;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.SonarException;

/**
 * Utility class that make it easier to run external tools.
 */
public class CommandExecutor {

  /**
   * Executes the external program represented by the command built from the list of strings.
   * 
   * @param command
   *          the list of string that compose the command to launch
   * @param timeoutSeconds
   *          the timeout for the program
   */
  public void execute(List<String> command, long timeoutSeconds) {
    execute(command.toArray(new String[command.size()]), timeoutSeconds);
  }

  /**
   * Executes the external program represented by the command built from the list of strings.
   * 
   * @param command
   *          the list of string that compose the command to launch
   * @param timeoutSeconds
   *          the timeout for the program
   */
  public void execute(String[] command, long timeoutSeconds) {
    ExecutorService executorService = null;
    Process process = null;
    String commandLine = StringUtils.join(command, " ");
    try {
      LoggerFactory.getLogger(getClass()).debug("Executing command: " + commandLine);
      ProcessBuilder builder = new ProcessBuilder(command);
      process = builder.start();

      // consume and display the error and output streams
      StreamGobbler outputGobbler = new StreamGobbler(process.getInputStream());
      StreamGobbler errorGobbler = new StreamGobbler(process.getErrorStream());
      outputGobbler.start();
      errorGobbler.start();

      final Process finalProcess = process;
      Callable<Integer> call = new Callable<Integer>() {

        public Integer call() throws Exception {
          finalProcess.waitFor();
          return finalProcess.exitValue();
        }
      };

      executorService = Executors.newSingleThreadExecutor();
      Future<Integer> ft = executorService.submit(call);
      int exitVal = ft.get(timeoutSeconds, TimeUnit.SECONDS);

      if (exitVal != 0) {
        throw new SonarException("External program execution failed.");
      }
    } catch (TimeoutException to) {
      if (process != null) {
        process.destroy();
      }
      throw new SonarException("Timeout exceeded: " + timeoutSeconds + " sec., command=" + commandLine, to);

    } catch (InterruptedException e) {
      throw new SonarException("Failed to execute command: " + commandLine, e);

    } catch (ExecutionException e) {
      throw new SonarException("Failed to execute command: " + commandLine, e);

    } catch (IOException e) {
      throw new SonarException("Failed to execute command: " + commandLine, e);

    } finally {
      if (executorService != null) {
        executorService.shutdown();
      }
    }
  }

  static class StreamGobbler extends Thread {

    InputStream is;

    StreamGobbler(InputStream is) {
      this.is = is;
    }

    @Override
    public void run() {
      Logger logger = LoggerFactory.getLogger(CommandExecutor.class);
      InputStreamReader isr = new InputStreamReader(is);
      BufferedReader br = new BufferedReader(isr);
      try {
        String line;
        while ((line = br.readLine()) != null) {
          logger.info(line);
        }
      } catch (IOException ioe) {
        logger.error("Error while reading output", ioe);
      } finally {
        IOUtils.closeQuietly(br);
        IOUtils.closeQuietly(isr);
      }
    }
  }
}
