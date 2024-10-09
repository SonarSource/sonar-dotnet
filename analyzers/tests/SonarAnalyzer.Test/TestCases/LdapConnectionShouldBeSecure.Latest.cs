using System;
using System.DirectoryServices;

new DirectoryEntry("path", "user", "pass", AuthenticationTypes.Secure); // Compliant
new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None);   // Noncompliant

var authTypeSecure = AuthenticationTypes.Secure;
new DirectoryEntry("path", "user", "pass", authTypeSecure); // Compliant

var authTypeNone = AuthenticationTypes.None;
new DirectoryEntry("path", "user", "pass", authTypeNone);   // Noncompliant

DirectoryEntry entry1 = new("path", "user", "pass", AuthenticationTypes.Secure);    // Compliant
DirectoryEntry entry2 = new("path", "user", "pass", AuthenticationTypes.None);      // Noncompliant
DirectoryEntry entry3 = new("path", "user", "pass", authTypeSecure);    // Compliant
DirectoryEntry entry4 = new("path", "user", "pass", authTypeNone);      // Noncompliant
// https://sonarsource.atlassian.net/browse/NET-412
DirectoryEntry entry5 = new("path", "user", "pass", AuthenticationTypes.None); // Noncompliant FP

(entry1.AuthenticationType, var x1) = (AuthenticationTypes.None, 0); // Noncompliant
(entry5.AuthenticationType, var x2) = (AuthenticationTypes.Secure, 0);
(entry5.AuthenticationType, var x3) = ((AuthenticationTypes.None), 0); // Noncompliant

record FieldsAndProperties
{
    private DirectoryEntry field1 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant {{Set the 'AuthenticationType' property of this DirectoryEntry to 'AuthenticationTypes.Secure'.}}
    private DirectoryEntry field2 = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
    private DirectoryEntry field3 = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
    private DirectoryEntry field4;
    private DirectoryEntry field5;
    private DirectoryEntry field6;
    private DirectoryEntry field7 = new DirectoryEntry();

    private DirectoryEntry Property0 => new DirectoryEntry { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
    private DirectoryEntry Property1 { get; set; } = new DirectoryEntry { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
    private DirectoryEntry Property3 { get; set; } = new DirectoryEntry() { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
    private DirectoryEntry Property4 { get; set; }
    private DirectoryEntry Property5 { get; set; }
    private DirectoryEntry Property6 { get; set; }

    private AuthenticationTypes AuthenticationType
    {
        set => field7.AuthenticationType = value;
        get => field7.AuthenticationType;
    }

    public void Method()
    {
        new DirectoryEntry("path", "user", "pass", AuthenticationTypes.Secure); // Compliant
        new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant
    }

    void Cases()
    {
        field3.AuthenticationType = AuthenticationTypes.None; // Noncompliant

        field4 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
        field4.AuthenticationType = AuthenticationTypes.Secure;

        // this
        this.field5 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
        this.field5.AuthenticationType = AuthenticationTypes.Secure;

        field6 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Noncompliant

        Property3.AuthenticationType = AuthenticationTypes.None; // Noncompliant
        Property4 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
        Property4.AuthenticationType = AuthenticationTypes.Secure;

        // this
        this.Property5 = new DirectoryEntry("path", "user", "pass", AuthenticationTypes.None); // Compliant, AuthenticationType is set on the next line
        this.Property5.AuthenticationType = AuthenticationTypes.Secure;

        Property6 = new DirectoryEntry(); // Compliant
    }
}

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

public partial class PartialProperty
{
    private partial DirectoryEntry Property1 { get; }
    private partial DirectoryEntry Property2 { get; }
    private partial DirectoryEntry this[int index] { get; }
}

public partial class PartialProperty
{
    private partial DirectoryEntry Property1
    {
        get => new DirectoryEntry { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
    }
    private partial DirectoryEntry Property2
    {
        get => new DirectoryEntry() { AuthenticationType = AuthenticationTypes.Secure }; // Compliant
    }
    private partial DirectoryEntry this[int index]
    {
        get => new DirectoryEntry { AuthenticationType = AuthenticationTypes.None }; // Noncompliant
    }
}
