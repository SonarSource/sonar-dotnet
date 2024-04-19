
// This is not expected to raise when the whole project is configured with nullable context.

// Noncompliant@+1 {{Do not use nullable context.}}
  #nullable enable
//^^^^^^^^^^^^^^^^

#nullable disable // Compliant

#nullable restore // Compliant
