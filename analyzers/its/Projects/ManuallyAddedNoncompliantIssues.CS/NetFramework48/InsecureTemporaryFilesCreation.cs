﻿/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.IO;

namespace NetFramework48
{
    public class InsecureTemporaryFilesCreation
    {
        public void Noncompliant()
        {
            var tempPath = Path.GetTempFileName();  // Noncompliant (S5445)

            using (var writer = new StreamWriter(tempPath))
            {
                writer.WriteLine("content");
            }
        }

        public void Compliant()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var fileStream = new FileStream(randomPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.DeleteOnClose))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("content");
            }
        }
    }
}
