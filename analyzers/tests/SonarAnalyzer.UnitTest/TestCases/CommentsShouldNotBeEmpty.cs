using System;

[Obsolete] // Ipsem Lorum
public class SingleLine //
// Noncompliant@-1 (inline comment)
{
             //
    // Noncompliant@-1 (a lot of whitespace before)

    //               
    // Noncompliant@-1 (a lot of whitespace after)

    // *
    // Ipsem Lorum
    //
    // Noncompliant @-1

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
