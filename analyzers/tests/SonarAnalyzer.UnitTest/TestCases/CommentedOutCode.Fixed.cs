// Copyright © 2011 - Present RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");

using System;


using System;
// Fixed

using System;
// Fixed

using System;
// Fixed

using System;
// Fixed

using System;
// foo ; {} bar
using System;
// ; {} foo
using System;

using System;

using System;

using System;

using System;

using System;

using System;
// Fixed

using System;
// Fixed

using System;
// &&
// ||
// && &&
// && ||
using System;

using System;

using System;

using System;
/*

    hello

    // Fixed

    ; world

    || && ||

    ;

*/

// Fixed
// Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");

// Console.WriteLine("Hello, world!");

/*




    // Fixed


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

            // Fixed
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
            // Fixed
            //
            //if (storage.HasFlag(StorageClass.Member))
            //{
            // output |= this.MemberTypeName.DisplayOn(builder, s);
            // builder.Append(CppNameBuilder.NameSeparator);
            // // Not trailing space wanted
            // output = false;
            //}
        }
 

        int a; // Fixed
        int b; // Fixed

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
        // Fixed
        // {this, is, a ,set; }
        int f;
        // Fixed
        // {Command();}
        int g;
        // Fixed
        // {Command(); }
        int h;
        // Fixed
        // int Method() { }
        int i;
        // Fixed
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
