/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System;
using System.IO;

namespace NetFramework48
{
    public class PubliclyWritableDirectoriesTest
    {
        public void NonCompliant(string partOfPath)
        {
            var tmp = Path.GetTempPath(); // Noncompliant (S5443)
            tmp = Path.GetTempPath(); // Noncompliant
            tmp = Environment.GetEnvironmentVariable("TMPDIR"); // Noncompliant
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            tmp = Environment.GetEnvironmentVariable("TMP"); // Noncompliant
            tmp = Environment.GetEnvironmentVariable("TEMP"); // Noncompliant
            tmp = Environment.ExpandEnvironmentVariables("%TMPDIR%"); // Noncompliant
            tmp = Environment.ExpandEnvironmentVariables("%TMP%"); // Noncompliant
            tmp = Environment.ExpandEnvironmentVariables("%TEMP%"); // Noncompliant
            tmp = "%USERPROFILE%\\AppData\\Local\\Temp\\f"; // Noncompliant
            tmp = "%TEMP%\\f"; // Noncompliant
            tmp = "%TMP%\\f"; // Noncompliant
            tmp = "%TMPDIR%\\f"; // Noncompliant
//                ^^^^^^^^^^^^^

            tmp = $"/tmp/{partOfPath}"; // Noncompliant
            //    ^^^^^^^^^^^^^^^^^^^^
        }
    }
}
