using System;

/*
hello
todo - Noncompliant
TODO */ //Noncompliant

// Noncompliant: hello TODO world

// ok


///
/// <summary>
/// TODO - Noncompliant
/// </summary>

/**
toDo - Noncompliant
*/

// The following should be compliant:
// aaaTODO000

// !TODO! Noncompliant

/*TODO*/ // Noncompliant
//^^^^

/**
*/
namespace Tests.Diagnostics
{
}