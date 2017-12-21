using System;

/*
hello
fixme - Noncompliant
FIXME */ //Noncompliant

// Noncompliant: hello FIXME world

// ok


///
/// <summary>
/// FIXME - Noncompliant
/// </summary>

/**
fixMe - Noncompliant
*/

// The following should be compliant:
// aaaFIXME000

// !FIXME! Noncompliant

/*FIXME*/ // Noncompliant
//^^^^^

/**
*/
namespace Tests.Diagnostics
{
}
