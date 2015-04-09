// Noncompliant: ;
using System;

// Noncompliant: ; 

// Noncompliant: {

// Noncompliant: }

// foo ; {} bar 

// ; {} foo

// Noncompliant: ++

// Noncompliant: for    (

// Noncompliant: if (

// Noncompliant: while(

// Noncompliant: catch(

// Noncompliant: switch(

// Noncompliant: try{

// Noncompliant: else{

// &&
// ||
// && &&
// && ||

// Noncompliant: && && &&

// Noncompliant: || || ||

// Noncompliant: || && ||

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
//
// Noncompliant: Console.WriteLine("Hello, world!");

/* Noncompliant: Console.WriteLine(); */

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
        public void M() {
            /* foo */ M();
            M(); /* foo */
        }


        int a; // Noncompliant: Console.WriteLine();
        int b; // Noncompliant: Console.WriteLine();
    }
}
