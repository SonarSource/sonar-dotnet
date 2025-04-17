using System;

namespace Tests.Diagnostics
{
    public enum MyEnum
    {
        Value
    }
    public enum myEnum // Noncompliant {{Rename the enumeration 'myEnum' to match the regular expression: '^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$'.}}
//              ^^^^^^
    {
        Value
    }
    public enum MyEnumTTTT // Noncompliant
    {
        Value
    }
    [Flags]
    public enum MyFlagEnums
    {
        Value
    }
    [Flags]
    public enum MyFlagEnum // Noncompliant {{Rename the enumeration 'MyFlagEnum' to match the regular expression: '^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?s$'.}}
//              ^^^^^^^^^^
    {
        Value
    }
    [Flags]
    public enum myFlagEnums // Noncompliant
    {
        Value
    }
    [Flags]
    public enum MyFlagEnumTTTTs // Noncompliant
    {
        Value
    }
}
