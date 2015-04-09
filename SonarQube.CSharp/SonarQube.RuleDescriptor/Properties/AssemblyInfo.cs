using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("SonarQube.RuleDescriptor")]
[assembly: AssemblyProduct("SonarQube.RuleDescriptor")]
[assembly: AssemblyDescription("")]

#if CodeSigned
[assembly: InternalsVisibleTo("SonarQube.Rules.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010029d15920332ded89851197f2ef16bfebd9cfc0acd7e0f3f5bbdc0d0ae03e7a893820e693e2ee9d886b362da373a6cd69e6041894fba4ea73b4c1ea31d1d6f2bd2b5a108f8863d0e01d52c58f29949719015b2889cc9f5057d7a802617d11f4c344dba9aae6d262b79c5220987b08ec0bfd9e39b0bb008441fa37b3f3b89814b8")]
#else
[assembly: InternalsVisibleTo("SonarQube.Rules.Test")]
#endif