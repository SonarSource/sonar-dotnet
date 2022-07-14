using System;
using System.DirectoryServices;

DirectoryEntry entry1 = new("path", "user", "pass", AuthenticationTypes.Secure);
DirectoryEntry entry2 = new("path", "user", "pass", AuthenticationTypes.None); // Noncompliant FP

(entry1.AuthenticationType, var x1) = (AuthenticationTypes.None, 0); // Noncompliant
(entry2.AuthenticationType, var x2) = (AuthenticationTypes.Secure, 0);
(entry2.AuthenticationType, var x3) = ((AuthenticationTypes.None), 0); // Noncompliant

public record struct RecordStruct
{
    public void SetValueAfterObjectInitialization()
    {
        DirectoryEntry entry1 = new("path", "user", "pass", AuthenticationTypes.None); // Compliant, property is set below
        (entry1.AuthenticationType, var x1) = (AuthenticationTypes.Secure, 0);

        DirectoryEntry entry2 = new("path", "user", "pass", AuthenticationTypes.None); // Noncompliant
        (entry2.AuthenticationType, var x2) = (AuthenticationTypes.None, 0); // Noncompliant

        AuthenticationTypes AuthenticationType;
        (AuthenticationType, var x3) = (AuthenticationTypes.None, 0);
    }
}
