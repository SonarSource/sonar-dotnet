using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// Issue #1419 - false positive in S1192 - shouldn't look at string in attributes
// https://github.com/SonarSource/sonar-dotnet/issues/1419

// compliant - outside class and in attribute -> ignored
[assembly: DebuggerDisplay("foo", Name = "foo", TargetTypeName = "foo")]

namespace Tests.Diagnostics
{
    public class ConstantsInAttributesShouldBeIgnored
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Compliant - ignored completely
        private string field1;

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private string field2;

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private string field3;

        // Compliant - repetition below threshold - string in attributes are not counted
        private string[] values = new[] { "Microsoft.Design", "Microsoft.Design" };

        // nonCompliant - repetition above threshold. String in attributes should not be highlighted
        private string[] values2 = new[] { "CA1024:UsePropertiesWhereAppropriate",  // Noncompliant {{Define a constant instead of using this literal 'CA1024:UsePropertiesWhereAppropriate' 3 times.}}
            "CA1024:UsePropertiesWhereAppropriate",                                 // Secondary
            "CA1024:UsePropertiesWhereAppropriate" };                               // Secondary
    }

    [DebuggerDisplay("12345", Name = "12345", TargetTypeName = "12345")] // Compliant - in attribute -> ignored
    public class Class1
    {
        [DebuggerDisplay("12345", Name = "12345", TargetTypeName = "12345")]
        string field1 = "12345"; // Noncompliant {{Define a constant instead of using this literal '12345' 4 times.}}

        [DebuggerDisplay("12345", Name = "12345", TargetTypeName = "12345")]
        private string Name { get; } = "12345";
//                                     ^^^^^^^ Secondary

        [DebuggerStepThrough]
        public bool DoStuff(string arg = "12345")
//                                       ^^^^^^^ Secondary
        {
            if (arg == "12345")
//                     ^^^^^^^ Secondary
            {
                return true;
            }
            return false;
        }
    }
}
