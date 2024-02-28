// Copyright © 2011 - Present RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// Noncompliant {{Remove this commented out code.}} : ;
using System;

// Noncompliant: ;
using System;
// Noncompliant@+1
// {
using System;
// Noncompliant@+1
// }
using System;
// Noncompliant@+1
// ;}
using System;
// Noncompliant@+1
// ; }
using System;
// foo ; {} bar
using System;
// ; {} foo
using System;
// Noncompliant: ++
using System;
// Noncompliant: for    ( .. i != 5
using System;
// Noncompliant: if ( 1==2
using System;
// Noncompliant: while( i > 5
using System;
// Noncompliant: catch(
using System;
// Noncompliant: switch(
using System;
// Noncompliant@+1
// try{
using System;
// Noncompliant@+1
// else{
using System;
// &&
// ||
// && &&
// && ||
using System;
// Noncompliant: && && &&
using System;
// Noncompliant: || || ||
using System;
// Noncompliant: || && ||
using System;
/*

    hello

    // Noncompliant: ;

    ; world

    || && ||

    ;

*/

// Noncompliant: Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");

// Console.WriteLine("Hello, world!");

/*




    // Noncompliant: Console.WriteLine();


    Console.WriteLine();
    */

namespace Tests.Diagnostics
{


    /// <summary>
    /// ...
    /// </summary>
    /// <code>
    /// Console.WriteLine("Hello, world!");
    /// </code>
    public class CommentedOutCode
    {
        public void M()
        {
            /* foo */
            M();
            M(); /* foo */

            // Noncompliant: Console.WriteLine("Hello, world!");
            // Console.WriteLine("Hello, world!");
            // Console.WriteLine("Hello, world!");

            // Console.WriteLine("Hello, world!"); //this is compliant, as there is code above and newline above

            M();
            /// Console.WriteLine("Hello, world!");
            ///
            ///


            /// The C++ access level for a member function, e.g. private
            ///

            M();
            // Noncompliant: Debug.Assert(this.MemberTypeName != null == storage.HasFlag(StorageClass.Member));
            //
            //if (storage.HasFlag(StorageClass.Member))
            //{
            // output |= this.MemberTypeName.DisplayOn(builder, s);
            // builder.Append(CppNameBuilder.NameSeparator);
            // // Not trailing space wanted
            // output = false;
            //}
        }


        int a; // Noncompliant: Console.WriteLine();
        int b; // Noncompliant: Console.WriteLine();

        // this should be compliant:
        // does *not* overwrite file if (still) exists

        //  https://github.com/SonarSource/sonar-dotnet/issues/2772
        int c;
        // It's just a URL and it is not an interpolated string
        // http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

        int rx;
        // regex{2,5}
        int d;
        // {this, is, a ,set}
        int e;
        // Noncompliant@+1
        // {this, is, a ,set; }
        int f;
        // Noncompliant@+1
        // {Command();}
        int g;
        // Noncompliant@+1
        // {Command(); }
        int h;
        // Noncompliant@+1
        // int Method() { }
        int i;
        // Noncompliant@+1
        // int Method() {}
        int j;
        // Compliant, not a C# code, but a data sample
        // { "json": "fragment" }
    }

    /**
        <summary>
        ...
        </summary>
        <code>
        Console.WriteLine("Hello, world!");
        </code>
    */
    public class CommentedOutCode2
    {
    }

    // Some C++ reference
    class X { }
    // Some c++ reference
    class Y { }
    // Somec++ reference
    class Z { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8819
class Repro_8819
{
    void LineTerminators()
    {
        // Remark: separators are required to consider comments as independent sentences

        _ = "separator"; // Noncompliant@+1 FP
        // Natural language sentence with semicolon at the end;

        _ = "separator"; // Noncompliant@+1 FP
        // Natural language sentence with open-brace at the end{

        _ = "separator"; // Noncompliant@+2 FP
        // Natural language sentence preceding sentence with semicolon at the end
        // Natural language sentence with semicolon at the end;

        _ = "separator"; // Noncompliant@+1 FP
        // Natural language sentence with escaped semicolon at the end;
        // Natural language sentence following sentence with escaped semicolon at the end

        _ = "separator"; // Compliant: Natural language sentence with semicolon and escaped space at the end; \u0020
        _ = "separator"; // Compliant: Natural language sentence with semicolon and multiple escaped spaces at the end; \u0020\u0020
        _ = "separator"; // Compliant: Natural language sentence with escaped semicolon at the end\u003B
        _ = "separator"; // Compliant: Natural language sentence with escaped open-brace at the end\u007B
        _ = "separator"; // Compliant: Natural language sentence with escaped close-brace at the end\u007D
        _ = "separator"; // Compliant: Natural language sentence with semicolon; in the middle
        _ = "separator"; // Compliant: Natural language sentence with colon at the end:
        _ = "separator"; // Compliant: Natural language sentence with comma at the end,
        _ = "separator"; // Compliant: Natural language sentence with period at the end.
        _ = "separator"; // Compliant: Natural language sentence with close-brace at the end}

        _ = "separator"; // Noncompliant@+1 FP
        // The empty set is indicated either with ∅ or with {}
    }

    void CommentedOutCommentsSeparatedByEmptyLine()
    {
        // Noncompliant: var x = 42;

        // FN: var y = 3;
    }

    void CommentedOutCommentsSeparatedByMultiLineComment()
    {
        // Noncompliant: var x = 42;
        /* A multiline comment  between the two commented-out blocks */
        // Noncompliant: var y = 42;
    }

    void CommentedOutCommentsSeparatedByBlock()
    {
        // Noncompliant: var x = 42;
        { }
        // Noncompliant: var y = 42;
    }
}
