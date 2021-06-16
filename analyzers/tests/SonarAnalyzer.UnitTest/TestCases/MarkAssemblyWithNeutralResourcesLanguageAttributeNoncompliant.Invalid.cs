// Noncompliant {{Provide a 'System.Resources.NeutralResourcesLanguageAttribute' attribute for assembly 'project0'.}}

[assembly: System.Resources.NeutralResourcesLanguage("")]
[assembly: System.Resources.NeutralResourcesLanguage(42)]   // Error [CS1503] Argument 1: cannot convert from 'int' to 'string'
