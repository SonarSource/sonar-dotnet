using System;
using System.Collections.Generic;
using System.Collections;

namespace Tests.Diagnostics.CSharp8
{
    public class NullCoalescenceAssignment
    {
        public void NullCoalescenceOperator_Null()
        {
            string name = null;
            name = name ?? "Laxmi";
            name.ToString();
        }

        public void NullCoalescenceOperator_Null_Noncompliant1()
        {
            string name = null;
            name = name ?? null;
            name.ToString(); // Noncompliant {{'name' is null on at least one execution path.}}
        }

        public void NullCoalescenceOperator_Null_Noncompliant2()
        {
            string name = null;
            name = name ?? name.ToString(); // Noncompliant
        }

        public void NullCoalescenceAssignment_Null()
        {
            string name = null;
            name ??= name.ToString(); // Noncompliant
        }

        public void NullCoalescenceAssignment_NotNull_Noncompliant()
        {
            string name1 = null;
            string name2 = null;
            name1 ??= name2.ToString(); // Noncompliant {{'name2' is null on at least one execution path.}}
        }

        public void NullCoalescenceAssignment_NotNull()
        {
            string name1 = "";
            string name2 = null;
            name1 ??= name2.ToString(); //  Ok - name1 is not null
        }
    }

    public class NullablePrimitiveType
    {
        public void NullablePrimitiveType_FN()
        {
            int? i1 = null;
            i1 = i1 ?? i1.GetHashCode(); // FN - nullable primitive type not supported
        }
    }

    public class SwitchExpressions
    {
        public void Nullable_In_Arm_Noncompliant(string s)
        {
            var result = s switch
            {
                null => s.ToString(), // Noncompliant
                _ => s.ToString()
            };
        }

        public void AlwaysNull_Noncompliant(int val)
        {
            string result = val switch
            {
                1 => null,
                2 => null,
                _ => null
            };
            result.ToString(); // Noncompliant
        }

        public void MaybeNull_Noncompliant(int val)
        {
            string result = val switch
            {
                1 => "1",
                2 => "Not 1",
                _ => null
            };
            result.ToString(); // Noncompliant
        }

        public void AlwaysNonNull(int val)
        {
            string result = val switch
            {
                1 => "1",
                2 => "2",
                _ => "Neither 1 or 2"
            };
            result.ToString();
        }

        public void Nullable_In_Arm(string s)
        {
            var result = s switch
            {
                var x when x != null => s.ToString(),
                _ => s.ToString() // FN Switch expressions are not constrained (See #2949)
            };
        }
    }

    public class DefaultLiteral
    {
        void NoncompliantDefaultLiteral()
        {
            object obj = default;
            obj.ToString(); // Noncompliant
        }

        void CompliantDefaultLiteral()
        {
            int i = default;
            i.ToString();
        }
    }

    public interface IWithDefaultMembers
    {
        string NoncompliantDefaultInterfaceMethod(string obj) =>
            obj != null ? null : obj.ToLower(); // Noncompliant

        string CompliantDefaultInterfaceMethod(string obj) =>
            obj == null ? null : obj.ToLower();
    }

    public class LocalStaticFunctions
    {
        public void Method()
        {
            string LocalFunction(string obj) =>
                obj != null ? null : obj.ToLower(); //  Compliant - FN: local functions are not supported by the CFG

            static string LocalStaticFunction(string obj) =>
                obj != null ? null : obj.ToLower(); //  Compliant - FN: local functions are not supported by the CFG
        }
    }
}
