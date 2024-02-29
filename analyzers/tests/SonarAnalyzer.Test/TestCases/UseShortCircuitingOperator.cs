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
            var b = true | false;   // Noncompliant {{Correct this '|' to '||'.}}
            b = true & false;       // Noncompliant {{Correct this '&' to '&&'.}}
//                   ^
            b = true && false;

            var i = 1 | 2;
        }

        public void ExtendedText(bool parameter)
        {
            var local = true;
            var a = true | !true;                  // Noncompliant {{Correct this '|' to '||'.}}                                                                              Right hand side detected as constant
            a = true | parameter;                  // Noncompliant {{Correct this '|' to '||'.}}                                                                              Right hand side detected as parameter reference
            a = true | local;                      // Noncompliant {{Correct this '|' to '||'.}}                                                                              Right hand side detected as local reference
            a = true | field;                      // Noncompliant {{Correct this '|' to '||'.}}                                                                              Right hand side detected as field reference
            a = true | Property;                   // Noncompliant {{Correct this '|' to '||'.}}                                                                              Right hand side detected as property reference
            a = true | ReturnSomeBool();           // Noncompliant {{Correct this '|' to '||' and extract the right operand to a variable if it should always be evaluated.}} Right hand side detected as possible side effect
            a = true | !parameter;                 // Noncompliant {{Correct this '|' to '||' and extract the right operand to a variable if it should always be evaluated.}} Right hand side detected as possible side effect
            a = true | (parameter ? true : false); // Noncompliant {{Correct this '|' to '||' and extract the right operand to a variable if it should always be evaluated.}} Right hand side detected as possible side effect
        }

        private bool ReturnSomeBool() =>
            Environment.Is64BitOperatingSystem;

        public void Repro_8834(bool a, bool b, bool c)
        {
            // CodeFix should add parantheses to preserve operator precedence
            _ = a && b | c; // Noncompliant
            //         ^
        }
    }
}
