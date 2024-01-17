using System;
using System.Security.AccessControl;
using System.Security.Principal;

const string compliantPart1 = """Us""";
const string compliantPart2 = """er""";
const string compliant = $"""{compliantPart1}{compliantPart2}""";
const string nonCompliantPart1 = """Ever""";
const string nonCompliantPart2 = """yone""";
const string noncompliant = $"""{nonCompliantPart1}{nonCompliantPart2}""";

FileSecurity fileSecurity = null;


fileSecurity.SetAccessRule(new (compliant, FileSystemRights.ListDirectory, AccessControlType.Allow));
fileSecurity.AddAccessRule(new (noncompliant, FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
fileSecurity.SetAccessRule(new FileSystemAccessRule("""Everyone""", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
