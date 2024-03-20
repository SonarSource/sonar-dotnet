/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Security.AccessControl;

namespace NetFramework48
{
    public class LooseFilePermissions
    {
        public void UpdatePermissionsUsingAccessControl(FileSecurity fileSecurity, DirectorySecurity directorySecurity)
        {
            fileSecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Allow));

            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant (S2612) {{Make sure this permission is safe.}}

            directorySecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));

            directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
        }
    }
}
