using System;

public closed class ClosedAttribute : Attribute { }                     // Compliant, closed classes are implicitly abstract

public sealed class DerivedFromClosedAttribute : ClosedAttribute { }    // Noncompliant

[AttributeUsage(AttributeTargets.Class)]
public sealed class AnnotatedDerivedFromClosedAttribute : ClosedAttribute { }
