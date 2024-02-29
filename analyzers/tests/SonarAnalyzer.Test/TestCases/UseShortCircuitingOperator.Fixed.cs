using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class UseShortCircuitingOperator
    {
        private bool field;
        private bool Property { get; }

        public UseShortCircuitingOperator()
        {
            var b = true || false;   // Fixed
            b = true && false;       // Fixed
            b = true && false;

            var i = 1 | 2;
        }

        public void ExtendedText(bool parameter)
        {
            var local = true;
            var a = true || !true;                  // Fixed
            a = true || parameter;                  // Fixed
            a = true || local;                      // Fixed
            a = true || field;                      // Fixed
            a = true || Property;                   // Fixed
            a = true || ReturnSomeBool();           // Fixed
            a = true || !parameter;                 // Fixed
            a = true || (parameter ? true : false); // Fixed
        }

        private bool ReturnSomeBool() =>
            Environment.Is64BitOperatingSystem;

        public void Repro_8834(bool a, bool b, bool c)
        {
            // CodeFix should add parantheses to preserve operator precedence
            _ = a && b || c; // Fixed
        }
    }
}
