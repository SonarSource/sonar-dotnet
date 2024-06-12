using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition_Switch
    {
        public int prop { get; set; }
        private void doTheRest() { throw new NotSupportedException(); }
        private void DoSomething() { throw new NotSupportedException(); }
        private void DoSomething(int i) { throw new NotSupportedException(); }
        private void DoSomethingDifferent() { throw new NotSupportedException(); }
        private void DoSomething2() { throw new NotSupportedException(); }

        public void Test_SingleLine()
        {
            var i = 5;
            switch (i)
            {
                case 1:
                    DoSomething(prop);
                    break;
                case 2:
                    DoSomethingDifferent();
                    break;
                case 3:
                    DoSomething(prop);
                    break;
                case 4:
                    {
                        DoSomething2();
                    }
                    break;
                case 5:
                    {
                        DoSomething2();
                        break;
                    }

                default:
                    DoSomething(prop);
                    break;
            }

            switch (i)
            {
                case 1:
                case 3:
                    DoSomething();
                    break;
                case 2:
                    DoSomethingDifferent();
                    break;
                default:
                    doTheRest();
                    break;
            }
        }

        public void Test_Multiline()
        {
            var i = 5;
            switch (i)
            {
                case 1: // Secondary
                    // Secondary@-1
                    DoSomething(prop);
                    DoSomething(prop);
                    break;
                case 2:
                    DoSomethingDifferent();
                    break;
                case 3:  // Noncompliant
                    DoSomething(prop);
                    DoSomething(prop);
                    break;
                case 4: // Secondary
                    {
                        DoSomething2();
                        DoSomething2();

                        break;
                    }
                case 5: // Noncompliant
                    {
                        DoSomething2();
                        DoSomething2();

                        break;
                    }
                case 6: // Secondary;
                    {
                        DoSomething2();
                    }
                    {
                        DoSomething2();
                    }
                    break;

                case 7: // Noncompliant
                    {
                        DoSomething2();
                    }
                    {
                        DoSomething2();
                    }
                    break;

                default: // Noncompliant
                    DoSomething(prop);
                    DoSomething(prop);
                    break;
            }

            int k = 0;
            switch (i)
            {
                case 1:
                case 3:
                    DoSomething();
                    DoSomething();
                    break;
                case 2:
                    DoSomethingDifferent();
                    DoSomethingDifferent();
                    break;
                case 4:
                    k++;
                    DoSomething();
                    DoSomethingDifferent();
                    break;
                case 5:
                    k++;
                    DoSomethingDifferent();
                    DoSomething();
                    break;
                default:
                    doTheRest();
                    doTheRest();
                    break;
            }
        }

        public int SwitchDifferentBranchDifferentOverloads(object o)
        {
            int result = 1;
            switch (o)
            {
                case float a: // Compliant
                    result++;
                    result = ValueConverter.ToInt32(a);
                    break;
                case bool a: // Compliant - ValueConverter.ToInt32 is an overload.
                    result++;
                    result = ValueConverter.ToInt32(a);
                    break;
                case int a: // // Secondary
                    result++;
                    result = ValueConverter.ToInt32(true);
                    break;
                case double a: // Noncompliant
                    result++;
                    result = ValueConverter.ToInt32(true);
                    break;
                case string a:
                    var flag = true;
                    result++;
                    result = ValueConverter.ToInt32(flag);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported array type");
            }
            return result;
        }

        public static class ValueConverter
        {
            public static int ToInt32(float f) => 0;
            public static int ToInt32(bool b) => 0;
        }

        public int SwitchDifferentSameAction(object o)
        {
            int result = 1;
            Action action = () => { };
            switch (o)
            {
                case float a: // Secondary
                    result++;
                    action();
                    break;
                case bool a: // Noncompliant
                    result++;
                    action();
                    break;
                case int a:
                    result++;
                    action.Invoke(); // FN - it's the same actions but invoked in a different way.
                    break;
                default:
                    throw new InvalidOperationException("Unsupported array type");
            }
            return result;
        }

        public int SwitchDifferentSameExtensionMethod(object o)
        {
            int result = 1;
            switch (o)
            {
                case float a:
                    result++;
                    result = result.FooInt(""); // FN - it's the same method with the same input invoked in different ways.
                    break;
                case bool a:
                    result++;
                    result = IntExtension.FooInt(result, "");
                    break;
                default:
                    throw new InvalidOperationException("Unsupported array type");
            }
            return result;
        }

        public void ExceptionOfException(int a)
        {
            switch (a)
            {
                case 1:  // Secondary [Exception]
                    DoSomething();
                    break;
                case 2: // Noncompliant [Exception]
                    DoSomething();
                    break;
            }

            switch (a)
            {
                case 1:
                    DoSomething();
                    break;
                case 2:
                    DoSomething();
                    break;
                default:
                    DoSomething();
                    break;
            }
        }

        public void Exception(int a)
        {
            switch (a)
            {
                case 10:
                    DoSomething();
                    break;
                case 20:
                    DoSomething2();
                    break;
                case 50:
                    DoSomething();
                    break;
            }

            switch (a)
            {
                case 10:
                    DoSomething();
                    break;
                case 20:
                    DoSomething2();
                    break;
                case 50:
                    DoSomething();
                    break;
                default:
                    DoSomething2();
                    break;
            }
        }

    }

    public static class IntExtension
    {
        public static int FooInt(this int a, string s) => 0;
    }
}
