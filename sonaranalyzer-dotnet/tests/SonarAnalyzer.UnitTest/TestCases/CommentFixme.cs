using System;

/*
hello
fixme -  // Noncompliant
FIXME */ // Noncompliant

// Noncompliant: hello FIXME world

// ok

// Noncompliant@+3
///
/// <summary>
/// FIXME -
/// </summary>

// Noncompliant@+2
/**
fixMe -
*/

// The following should be compliant:
// aaaFIXME000

// Noncompliant@+1
// !FIXME!

/*FIXME*/ // Noncompliant
//^^^^^

/**
*/
namespace Tests.Diagnostics
{
    class FixMe
    {
        public FixMe()
        {
        }
    }
}
