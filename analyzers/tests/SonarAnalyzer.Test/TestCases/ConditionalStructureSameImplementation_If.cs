using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition_If
    {
        public bool someCondition1 { get; set; }
        public bool someCondition2 { get; set; }
        public bool someCondition3 { get; set; }

        public void DoSomething1() { }
        public void DoSomething2() { }

        public void Test_SingleLineBlocks()
        {
            if (someCondition1)
            {
                DoSomething1(); // Compliant, ignore single line blocks
            }
            else
            {
                DoSomething1();
            }

            if (someCondition1)
                DoSomething1(); // Compliant, ignore single line blocks
            else
                DoSomething1();
        }

        public void Test_MultilineBlocks()
        {
            if (someCondition1)
            { // Secondary
                DoSomething1();
                DoSomething1();
            }
            else
            { // Noncompliant
                DoSomething1();
                DoSomething1();
            }

            if (someCondition1)
            { // Secondary
                // Secondary@-1
                DoSomething1();
                DoSomething1();
            }
            else if (someCondition2)
            { // Noncompliant
                DoSomething1();
                DoSomething1();
            }
            else if (someCondition3)
            {
                DoSomething2();
            }
            else
            { // Noncompliant
                DoSomething1();
                DoSomething1();
            }

            if (someCondition1)
            { // Secondary
                // Secondary@-1
                DoSomething1();
                DoSomething1();
            }
            else if (someCondition2)
            { // Noncompliant
                DoSomething1();
                DoSomething1();
            }
            else
            {// Noncompliant
                DoSomething1();
                DoSomething1();
            }
        }

        public void Test_Overloads()
        {
            int foo = 0;
            if (someCondition1)
            {
                foo++;
                foo = foo.FooInt(); // FN
            }
            else
            {
                foo++;
                foo = IntExtension.FooInt(foo);
            }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/1255
        public void ExceptionOfException(int a)
        {
            if (a == 1)
            { // Secondary [Exception]
                DoSomething1();
            }
            else if (a == 2)
            { // Noncompliant [Exception]
                DoSomething1();
            }
        }

        public void Exception(int a)
        {
            if (a >= 0 && a < 10)
            {
                DoSomething1();
            }
            else if (a >= 10 && a < 20)
            {
                DoSomething2();
            }
            else if (a >= 20 && a < 50)
            {
                DoSomething1();
            }
        }

        public bool ElseIfChain(int s)
        {
            if (s == 0)
            { // Secondary [IfChain1]
                DoSomething1();
                return true;
            }
            else if (s > 0 && s < 11)
            { // Noncompliant [IfChain1]
                DoSomething1();
                return true;
            }
            else if (s > 11 && s < 20)
            {
                DoSomething2();
                return true;
            }

            if (s == 0)
            { // Secondary [IfChain2]
                DoSomething1();
            }
            else if (s > 0 && s < 11)
            { // Noncompliant [IfChain2]
                // Secondary@-1 [IfChain3]
                DoSomething1();
            }
            else if (s > 11 && s < 20)
            { // Noncompliant [IfChain3]
                DoSomething1();
            }

            if (s == 0)
            {   // Compliant
                DoSomething1();
            }
            else
            {
                if (s > 0 && s < 11)
                {   // Compliant
                    DoSomething1();
                }
                else
                {
                    if (s > 11 && s < 20)
                    {   // Compliant
                        DoSomething1();
                    }
                    else
                    {
                        DoSomething2();
                    }
                }
            }

            if (s == 0)
            {   // Secondary [NestedIfChain]
                // Secondary@-1 [NestedIfChain2]
                DoSomething1();
                DoSomething1();
            }
            else
            {
                if (s > 0 && s < 11)
                {   // Noncompliant [NestedIfChain]
                    DoSomething1();
                    DoSomething1();

                }
                else
                {
                    if (s > 11 && s < 20)
                    {// Noncompliant [NestedIfChain2]

                        DoSomething1();
                        DoSomething1();
                    }
                }
            }

            if (s == 0)
            {   // Compliant as the DoSomething2() call breaks the conditional chain
                DoSomething1();
                DoSomething1();
            }
            else
            {
                DoSomething2();
                if (s > 0 && s < 11)
                {   // Compliant
                    DoSomething1();
                    DoSomething1();
                }
            }
                return false;
        }
    }

    public static class IntExtension
    {
        public static int FooInt(this int a) => 0;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9637
public class UnresolvedSymbols
{
    public string Method(string first, string second)
    {
        if (first.Length == 42)
        {
            var ret = UnknownMethod();  // Error [CS0103]
            return ret;
        }
        else if (second.Length == 42)
        {
            var ret = UnknownMethod();  // Error [CS0103]
            return ret;
        }
        return "";
    }
}
