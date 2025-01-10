using System;

[Obsolete] // Ipsem Lorum
public class SingleLine //
// hey

// Noncompliant@-3 (inline comment)
{ // Ipsem Lorum
             //

    // Noncompliant@-2 (a lot of whitespace before)

    //

    // Noncompliant@-2 (a lot of whitespace after)

    // *
    // Ipsem Lorum
    //

    // hey
    //
    //
    //

    //
    //
    // hey
    //
    //
    //

    //
    //
    //
    // hey
    //
    //
    //


    //

    // Noncompliant @-2

    // \r

    // \n

    // \r\n

    // \t

    // z̶̤͚̅̍a̷͈̤̪͌͛̈ļ̷̈͐͝g̸̰̈́͂̆o̴̓̏͜

    // /**/

    // ///

    // //

    // /** */

    //
    //
    // hey
    //
    //
    // there
    //
    //

    //
    //
    //
    /// text
    // Noncompliant@-4
    // Secondary@-4
    // Secondary@-4

    //
    //
    //
    /*
     * text
     */
    // Noncompliant@-6
    // Secondary@-6
    // Secondary@-6

    //
    //
    //
    /**
     * text
     */
    // Noncompliant@-6
    // Secondary@-6
    // Secondary@-6

    void Method()
    {
        // Noncompliant@+4 {{Remove this empty comment}}
        // Secondary@+4    {{Remove this empty comment}}
        // Secondary@+4    {{Remove this empty comment}}

        //
        //
        //
        var x = 42; //
        //
        //
        //

        // Noncompliant@-5
        // Noncompliant@-5
        // Secondary@-5
        // Secondary@-5

        //
        //
        // hello
        //
        //
        var y = 42; //
        //
        //
        //
        // there

        // Noncompliant@-6

        //
    } //
    //

    // Noncompliant@-4
    // Noncompliant@-4
    // Noncompliant@-4
}

[Obsolete] /// Ipsem Lorum
public class SingleLineDocumentation ///
// Noncompliant@-1 (inline comment)
{
            ///
    // Noncompliant@-1 (a lot of whitespace before)

    ///
    // Noncompliant@-1 (a lot of whitespace after)

    ///
    /// *
    /// Ipsem Lorum
    /// \\n
    ///

    ///
    /// hey there

    /// hey there
    /// text
    ///

    /// ///

    /// //

    /// /* */

    /// /** */

    /// \r

    /// \n

    /// \r\n

    /// \t

    /// z̶̤͚̅̍a̷͈̤̪͌͛̈ļ̷̈͐͝g̸̰̈́͂̆o̴̓̏͜

    ///
    ///
    ///
    // Noncompliant @-3
}

[Obsolete] /* Ipsem Lorum */
[Serializable] /**/
// Noncompliant @-1 (inline comment)
public class MultiLine /* */
// Noncompliant @-1 (inline comment)
{
            /* */
    // Noncompliant@-1 (a lot of whitespace before)

    /*      */
    // Noncompliant@-1 (a lot of whitespace inside)

    /**/
    // Noncompliant@-1 (a lot of whitespace after)

    // Noncompliant@+1
    /*
     *
     */

    // Noncompliant@+1
    /*
     *
     */

    // Noncompliant@+1
    /*
     *
     *
     *
     */


    // Noncompliant@+1
    /*

     */

    // Noncompliant@+1
    /*



     */

    /* hey
     */

    /*
     there */

    /* hey
     *
     */

    /*
     *
     there */

    /*

    Ipsem Lorum
    \n
    */

    /*
    hey there
    */

    /*
     //
     */

    /*
     ///
    */

    /*
    \r
    */

    /*
    \n
    */

    /*
    \r\n
    */

    /*
    \t
    */

    /*
    z̶̤͚̅̍a̷͈̤̪͌͛̈ļ̷̈͐͝g̸̰̈́͂̆o̴̓̏͜
    */
}

[Obsolete] /** Ipsem Lorum */
[Serializable] /***/
// Noncompliant @-1 (inline comment)
public class MultiLineDocumentation /** */
// Noncompliant @-1 (inline comment)
{
            /** */
    // Noncompliant@-1 (a lot of whitespace before)

    /**      */
    // Noncompliant@-1 (a lot of whitespace inside)

    /***/
    // Noncompliant@-1 (a lot of whitespace after)

    // Noncompliant@+1
    /**
     *
     */

    // Noncompliant@+1
    /**
     *
     *
     *
     */


    // Noncompliant@+1
    /**

     */

    // Noncompliant@+1
    /**



     */

    /** hey
     */

    /**
     there */

    /** hey
     *
     */

    /**
     *
     there */

    /**

    Ipsem Lorum
    \n
    */

    /**
    hey there
    */

    /**
     //
     */

    /**
     ///
    */

    /**
    \r
    */

    /**
    \n
    */

    /**
    \r\n
    */

    /**
    \t
    */

    /**
    z̶̤͚̅̍a̷͈̤̪͌͛̈ļ̷̈͐͝g̸̰̈́͂̆o̴̓̏͜
    */
}

//
// hey
//
//
// there
//

///
/// hey
///
/// there
///


// Noncompliant@+4
// Secondary@+4
// Secondary@+4

//
//
//

// Noncompliant@+1
///
///
