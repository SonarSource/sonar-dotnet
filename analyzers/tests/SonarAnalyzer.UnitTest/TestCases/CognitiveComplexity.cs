using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class MethodsComplexity
    {
        void Zero() { }

        void If() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {

            }
        }

        void IfElseIfElse() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (1 == 2)
//          ^^ Secondary {{+1}}
            {

            }
            else if (1 == 3)
//          ^^^^ Secondary {{+1}}
            {

            }
            else
//          ^^^^ Secondary {{+1}}
            {
            }
        }

        void IfNestedInElse() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {

            }
            else
//          ^^^^ Secondary {{+1}}
            {
                if (false)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {
                }
            }
        }

        void IfElseNestedInIf() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                if (true)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                }
                else
//              ^^^^ Secondary {{+1}}
                {

                }
            }
        }

        void MultipleIfNested() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 6 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
                if (true)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                    if (true)
//                  ^^ Secondary {{+3 (incl 2 for nesting)}}
                    {

                    }
        }

        void Switch() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            switch (10)
//          ^^^^^^ Secondary {{+1}}
            {
                case 1:
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }

        void NestedSwitch() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                switch (10)
//              ^^^^^^ Secondary {{+2 (incl 1 for nesting)}}
                {
                    case 1:
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
            }
        }

        void SwitchWithNestedIf() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            switch (10)
//          ^^^^^^ Secondary {{+1}}
            {
                case 0:
                    if (true)
//                  ^^ Secondary {{+2 (incl 1 for nesting)}}
                    {

                    }
                    break;
                default:
                    break;
            }
        }

        void SwitchExpression(int count) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            var x = count switch
//                        ^^^^^^ Secondary {{+1}}
            {
                0 => "zero",
                _ => "other"
            };
        }

        void NestedSwitchExpression(bool first, bool second) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            var x = first switch
//                        ^^^^^^ Secondary {{+1}}
            {
                true => second switch
//                             ^^^^^^ Secondary {{+2 (incl 1 for nesting)}}
                {
                    true => 1,
                    false => 2
                },
                _ => 3
            };
        }

        void MultipleSwitchExpression(bool first, bool second) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            var x = first switch
//                        ^^^^^^ Secondary {{+1}}
            {
                true => 1,
                _ => 2
            };

            x = second switch
//                     ^^^^^^ Secondary {{+1}}
            {
                true => 3,
                _ => 4
            };
        }

        void TernaryOperator() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            var t = true ? 0 : 1;
//                       ^ Secondary {{+1}}
        }

        void NestedTernaryOperator() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                var t = true ? 0 : 1;
//                           ^ Secondary {{+2 (incl 1 for nesting)}}
            }
        }

        void NullConditional() // Compliant - Null conditional operators should be ignored
        {
            var a = new int[1];

            var b = a?.Length;
            var c = a?[0];
        }

        void NullCoalescence() // Compliant - Null coalescence operators should be ignored
        {
            bool? value = null;

            value ??= true;
        }

        void NullCoalescenceAssignment() // Compliant - Null coalescence operators should be ignored
        {
            bool? value = null;

            value ??= true;
        }

        void TernaryOperatorWihtInnerTernayOperator() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            var t = true ? false ? -1 : 0 : 1;
//                       ^ Secondary {{+1}}
//                               ^ Secondary@-1 {{+2 (incl 1 for nesting)}}
        }

        void While() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            while (true)
//          ^^^^^ Secondary {{+1}}
            {

            }
        }

        void NestedWhile() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                while (true)
//              ^^^^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                }
            }
        }

        void For() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            for (int i = 0; i < 10; i++)
//          ^^^ Secondary {{+1}}
            {

            }
        }

        void NestedFor() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                for (int i = 0; i < 10; i++)
//              ^^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                }
            }
        }

        void Foreach() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            foreach (var item in Enumerable.Empty<int>())
//          ^^^^^^^ Secondary {{+1}}
            {
            }
        }

        void NestedForeach() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                foreach (var item in Enumerable.Empty<int>())
//              ^^^^^^^ Secondary {{+2 (incl 1 for nesting)}}
                {
                }
            }
        }

        void DoWhile() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            do
//          ^^ Secondary {{+1}}
            {

            } while (true);
        }

        void NestedDoWhile() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                do
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                } while (true);
            }
        }

        void TryCatch() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            try
            {

            }
            catch (Exception)
//          ^^^^^ Secondary {{+1}}
            {
                throw;
            }
        }

        void TryCatchIf() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            try
            {

            }
            catch (Exception)
//          ^^^^^ Secondary {{+1}}
            {
                if (true)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {
                    throw;
                }
            }
        }

        void NestedTryCatch() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                try
                {

                }
                catch (Exception)
//              ^^^^^ Secondary {{+2 (incl 1 for nesting)}}
                {
                    throw;
                }
            }
        }

        void TryCatchFinally() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            try
            {

            }
            catch (Exception)
//          ^^^^^ Secondary {{+1}}
            {
                throw;
            }
            finally
            {

            }
        }

        void TryFinally()
        {
            try
            {

            }
            finally
            {

            }
        }

        void EmptyMethodBody() => Console.Write("");

        bool IfMethodBody(bool a, bool b, bool c) => a && b || c;
//           ^^^^^^^^^^^^ {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
//                                                     ^^ Secondary@-1 {{+1}}
//                                                          ^^ Secondary@-2 {{+1}}

        void MethodWithInnerMethod() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            void InnerMethod() // Inner method increase the nesting by 1
            {
                if (true)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                }
            }
        }
    }

    class PropertiesComplexity
    {
        string SimpleProperty
        {
            get;
            set;
        }

        private string foo;
        string Foo
        {
            get { return foo; }
            set { foo = value; }
        }

        string IfInProperty
        {
            get // Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            {
                if (true)
//              ^^ Secondary {{+1}}
                {
                    return "foo";
                }
            }
        }

        string IfInPropertyGetSet
        {
            get // Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            {
                if (true)
//              ^^ Secondary {{+1}}
                {
                    return "foo";
                }
            }
            set // Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            {
                if (true)
//              ^^ Secondary {{+1}}
                {
                    foo = value;
                }
            }
        }

        string ArrowedProperty => string.Empty;
    }

    class EventsComplexity
    {
        event EventHandler Bar;
        event EventHandler Foo
        {
            add // Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            {
                if (true)
//              ^^ Secondary {{+1}}
                {
                    Bar += value;
                }
            }
            remove // Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            {
                if (true)
//              ^^ Secondary {{+1}}
                {
                    Bar -= value;
                }
            }
        }
    }

    class ConstructorsComplexity
    {
        ConstructorsComplexity()
        {

        }

        ConstructorsComplexity(string foo) // Noncompliant {{Refactor this constructor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            if (foo == null)
//          ^^ Secondary {{+1}}
            {
                throw new ArgumentNullException();
            }
        }
    }

    class DestructorsComplexity
    {
        ~DestructorsComplexity()
        {

        }
    }

    class DestructorsComplexity2
    {
        ~DestructorsComplexity2() // Noncompliant {{Refactor this destructor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {

            }
        }
    }

    class OperatorsComplexity
    {
        public static OperatorsComplexity operator +(OperatorsComplexity left, OperatorsComplexity right)
        {
            return null;
        }

        public static OperatorsComplexity operator -(OperatorsComplexity left, OperatorsComplexity right) // Noncompliant {{Refactor this operator to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {

            }
            return null;
        }
    }

    class RecursionsComplexity
    {
        void DirectRecursionComplexity()
//           ^^^^^^^^^^^^^^^^^^^^^^^^^ {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{+1 (recursion)}}
        {
            DirectRecursionComplexity();
        }

        void DirectRecursionComplexity_DifferentArguments()
        {
            DirectRecursionComplexity_DifferentArguments(1); // This is not recursion, no complexity increase
        }

        void DirectRecursionComplexity_DifferentArguments(int arg)
        {
        }

        void IndirectRecursionComplexity()
        {
            TmpIndirectRecursion();
        }

        void TmpIndirectRecursion()
        {
            IndirectRecursionComplexity();
        }

        void IndirectRecursionFromLocalLambda()
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{+1 (recursion)}}
        {
            Action act = () => IndirectRecursionFromLocalLambda();
            act();
        }
    }

    class AndOrConditionsComplexity
    {
        void SimpleAnd()
        {
            var a = true;
        }

        void SimpleAnd2() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            var a = true && false;
//                       ^^ Secondary {{+1}}
        }

        void SimpleOr() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            var a = true || false;
//                       ^^ Secondary {{+1}}
        }

        void SimpleNot()
        {
            var a = !true;
        }

        void AndOr() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            var a = true && false || true;
//                       ^^ Secondary {{+1}}
//                                ^^ Secondary@-1 {{+1}}
        }

        void AndOrIf(bool a, bool b, bool c, bool d, bool e, bool f) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
        {
            if (a
//          ^^ Secondary {{+1}}
                && b && c
//              ^^ Secondary {{+1}}
                || d || e
//              ^^ Secondary {{+1}}
                && f)
//              ^^ Secondary {{+1}}
            {

            }
        }

        void AndNotIf(bool a, bool b, bool c, bool d) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            var res =
                a
                &&
//              ^^ Secondary {{+1}}
                !(b && c)
//                  ^^ Secondary {{+1}}
                && d;
        }

        void AndOrNot1(bool a, bool b, bool c, bool d) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            var res =
                d
                ||
//              ^^ Secondary {{+1}}
                a
                &&
//              ^^ Secondary {{+1}}
                (!b || !c);
//                  ^^ Secondary {{+1}}
        }

        void AndOrNot2(bool a, bool b, bool c, bool d) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            var res =
                a
                &&
//              ^^ Secondary {{+1}}
                (!b || !c)
//                  ^^ Secondary {{+1}}
                || d;
//              ^^ Secondary {{+1}}
        }

        void AndNot3(bool a, bool b, bool c, bool d) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            var res = a && d && !(b && c);
//                      ^^ Secondary {{+1}}
//                                  ^^ Secondary@-1 {{+1}}
        }

        void AndNotParenthesis(bool a, bool b, bool c) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            var res =
                a
                &&
//              ^^ Secondary {{+1}}
                !(((b && c)));
//                    ^^ Secondary {{+1}}
        }

        void ChainedConditionsWithParentheses(bool a, bool b, bool c, bool d) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            var res = a && b && (c && d);
//                      ^^ Secondary {{+1}}
        }
    }

    class GotoComplexity
    {
        void Foo() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            goto Outer;
//          ^^^^ Secondary {{+1}}
            Outer:
            Console.WriteLine();
        }

        void Bar() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            switch (5)
//          ^^^^^^ Secondary {{+1}}
            {
                case 1000:
                    goto case 100;
//                  ^^^^ Secondary {{+2 (incl 1 for nesting)}}
                case 100:
                    break;
                default:
                    break;
            }
        }
    }

    class LambdasComplexity
    {
        private Action<int> act = // Noncompliant {{Refactor this field to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
            x =>
            {
                if (x > 0)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                }
            };

        private Func<int, string> act2 = // Noncompliant {{Refactor this field to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
            x =>
            {
                if (x > 0)
//              ^^ Secondary {{+2 (incl 1 for nesting)}}
                {

                }
                return "";
            };

        void SimpleFunc()
        {
            Func<int, string> func = x => "";
        }

        void BlockFunc()
        {
            Func<int, string> func = x =>
            {
                return "";
            };
        }

        void IfFunc() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            Func<int, string> func =
                x =>
                {
                    if (x > 0)
//                  ^^ Secondary {{+2 (incl 1 for nesting)}}
                    {
                        return "";
                    }

                    return "";
                };
        }

        void SimpleAction()
        {
            Action<string> act = x => Console.Write(x);
        }

        void BlockAction()
        {
            Action<string> func = x =>
            {
                Console.Write(x);
            };
        }

        void IfAction() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        {
            Action<int> func =
                x =>
                {
                    if (x > 0)
//                  ^^ Secondary {{+2 (incl 1 for nesting)}}
                    {
                    }
                };
        }
    }
}
