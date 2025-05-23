﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

MyDelegate first, second, third, fourth;
first = () => Console.Write("1");
second = () => Console.Write("2");
third = () => Console.Write("3");
fourth = () => Console.Write("4");

MyDelegate chain1234 = first + second + third + fourth; // Compliant - chain sequence = "1234"
MyDelegate chain12 = chain1234 - third - fourth; // Compliant - chain sequence = "12"
chain12 = chain1234 - (third + third) - fourth; // Noncompliant {{Review this subtraction of a chain of delegates: it may not work as you expect.}}
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

public delegate void MyDelegate();

record R
{
    MyDelegate first, second, third, fourth;
    string Property
    {
        init
        {
            MyDelegate chain1234 = first + second + third + fourth; // Compliant - chain sequence = "1234"
            MyDelegate chain12 = chain1234 - third - fourth; // Compliant - chain sequence = "12"
            chain12 = chain1234 - (third + third) - fourth; // Noncompliant {{Review this subtraction of a chain of delegates: it may not work as you expect.}}
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8467
class Repro_8467
{
    void SwitchStatement(Action action)
    {
        action -= 1 switch // Noncompliant FP
        {
            1 => action,
            2 => action,
        };
    }
}
