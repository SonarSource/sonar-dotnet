using System;

[Obsolete] // Ipsem Lorum
public class SimpleLineComment //
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
}
