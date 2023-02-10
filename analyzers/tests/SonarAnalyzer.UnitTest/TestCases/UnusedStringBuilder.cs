﻿using System.Collections.Generic;
using System.Text;

public class Program
{
    public StringBuilder MyMethod(StringBuilder builder, string myString) // Compliant
    {
        StringBuilder sb1 = GetStringBuilder(); // Compliant

        var sb2 = new StringBuilder(); // Compliant
        ExternalMethod(sb2);

        var sb3 = new StringBuilder(); // Compliant
        LocalMethod(sb3);

        var sb4 = new StringBuilder(); // Compliant

        StringBuilder sb5 = new StringBuilder(); // Noncompliant {{Remove this "StringBuilder"; ".ToString()" is never called.}}
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^
        var sb6 = new StringBuilder(); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^

        StringBuilder sb7 = new StringBuilder(); // Noncompliant
        sb7.Append(sb4.ToString());

        StringBuilder sb8 = new StringBuilder(); // Compliant
        StringBuilder sb9 = new StringBuilder(); // Noncompliant
        sb9 = sb8;

        if (true)
        {
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            var buildernested = new StringBuilder(); // Noncompliant
                        }
                    }
                }
            }
        }

        (StringBuilder, StringBuilder) builderTuple = (new StringBuilder(), new StringBuilder()); // FN

        StringBuilder builderInLine1 = new StringBuilder(), builderInLine2 = new StringBuilder(); // Noncompliant
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  // Noncompliant@-1

        StringBuilder builderCfg = new StringBuilder(); // FN (requires use of cfg with significant impact on performance)
        if (false)
        {
            builderCfg.ToString();
        }

        StringBuilder builderCalls = new StringBuilder(); // Noncompliant
        builderCalls.Append("Append");
        builderCalls.AppendLine("AppendLine");
        builderCalls.Replace("\r\n", "\n");
        builderCalls.Clear();
        builderCalls.GetType();

        StringBuilder mySbField = new StringBuilder(); // Noncompliant
        MyClass myClass = new MyClass();
        myClass.mySbField.ToString();

        var builderReturn = new StringBuilder(); // Compliant
        return builderReturn;

        void LocalMethod(StringBuilder local)
        {
            local.ToString();
        }
    }

    public StringBuilder GetStringBuilder() => // Compliant
        new StringBuilder();

    public void ExternalMethod(StringBuilder builder) { } // Compliant

    public string AnotherMethod()
    {
        var builder = new StringBuilder(); // FP
        return $"{builder} is ToStringed here";
    }

    public string MyProperty
    {
        get
        {
            var builder1 = new StringBuilder(); // Noncompliant
            var builder2 = new StringBuilder(); // Compliant
            return builder2.ToString();
        }
    }

    private StringBuilder myField = new StringBuilder(); // Compliant
}

public class MyClass
{
    public StringBuilder mySbField = new StringBuilder();
}
