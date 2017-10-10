// Copyright © 2011 - Present RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// Noncompliant: ; {{Remove this commented out code.}}
using System;

// Noncompliant: ;
using System;
// Noncompliant: {
using System;
// Noncompliant: }
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
// Noncompliant: try{
using System;
// Noncompliant: else{
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

    Noncompliant: ;

    ; world

    || && ||

    ;

*/

// Noncompliant: Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");

// Console.WriteLine("Hello, world!");

/*




    Noncompliant: Console.WriteLine();


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
