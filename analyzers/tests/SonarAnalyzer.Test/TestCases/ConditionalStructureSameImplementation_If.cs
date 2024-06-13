﻿using System;
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

        public bool FalseNegative(string s, out int kind)
        {
            kind = 0;
            if (s.StartsWith("__Iterator", StringComparison.Ordinal))
            { // Secondary [Following]
                kind = 42;
                return true;
            }
            else if (s.StartsWith("__async", StringComparison.Ordinal))
            { // Noncompliant [Following]
                kind = 42;
                return true;
            }
            else if (s.StartsWith("__AnonStorey", StringComparison.Ordinal))
            {
                kind = 21;
                return true;
            }

            while (true)
            {
                if (s == string.Empty)
                { // Secondary [FN3]
                    kind = 42;
                }
                else if (s.StartsWith("__async", StringComparison.Ordinal))
                { // Noncompliant [FN3]
                // Secondary@-1 [TP4]
                    kind = 42;
                }
                else if (s.StartsWith("__AnonStorey", StringComparison.Ordinal))
                { // Noncompliant [TP4]
                    kind = 42;
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
