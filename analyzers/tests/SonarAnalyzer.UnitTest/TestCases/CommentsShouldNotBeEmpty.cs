using System;

[Obsolete] // Ipsem Lorum
public class SingleLineComment //
// Noncompliant@-1 (inline comment)
{
        //
//               
// Noncompliant@-2 (a lot of whitespace before)
// Noncompliant@-2 (a lot of whitespace after)

// *
// Ipsem Lorum
// \n
//
// Noncompliant @-1

//

// Noncompliant @-2 

// /**/

// ///

// //

// /** */
}

[Obsolete] /// Ipsem Lorum
public class SingleLineDocumentationComment ///
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
