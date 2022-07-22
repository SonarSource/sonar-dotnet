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
        private void doSomething() { throw new NotSupportedException(); }
        private void doSomething(int i) { throw new NotSupportedException(); }
        private void doSomethingDifferent() { throw new NotSupportedException(); }
        private void doSomething2() { throw new NotSupportedException(); }

        public void Test_SingleLine()
        {
            var i = 5;
            switch (i)
            {
                case 1:
                    doSomething(prop);
                    break;
                case 2:
                    doSomethingDifferent();
                    break;
                case 3:
                    doSomething(prop);
                    break;
                case 4:
                    {
                        doSomething2();
                    }
                    break;
                case 5:
                    {
                        doSomething2();
                        break;
                    }

                default:
                    doSomething(prop);
                    break;
            }

            switch (i)
            {
                case 1:
                case 3:
                    doSomething();
                    break;
                case 2:
                    doSomethingDifferent();
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
                    doSomething(prop);
                    doSomething(prop);
                    break;
                case 2:
                    doSomethingDifferent();
                    break;
                case 3:  // Noncompliant
                    doSomething(prop);
                    doSomething(prop);
                    break;
                case 4: // Secondary
                    {
                        doSomething2();
                        doSomething2();

                        break;
                    }
                case 5: // Noncompliant
                    {
                        doSomething2();
                        doSomething2();

                        break;
                    }
                case 6: // Secondary;
                    {
                        doSomething2();
                    }
                    {
                        doSomething2();
                    }
                    break;

                case 7: // Noncompliant
                    {
                        doSomething2();
                    }
                    {
                        doSomething2();
                    }
                    break;

                default: // Noncompliant
                    doSomething(prop);
                    doSomething(prop);
                    break;
            }

            switch (i)
            {
                case 1:
                case 3:
                    doSomething();
                    doSomething();
                    break;
                case 2:
                    doSomethingDifferent();
                    doSomethingDifferent();
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

    }

    public static class IntExtension
    {
        public static int FooInt(this int a, string s) => 0;
    }
}
