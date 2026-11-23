using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CA = System.Diagnostics.CodeAnalysis;
using Sys = System;

namespace Net6Poc.IssueSuppression
{
    internal class TestCases
    {
        public void Bar(IEnumerable<int> collection)
        {
            [SuppressMessage("", "")] int Get() => 1; // Noncompliant

            _ = collection.Select([SuppressMessage("", "")] (x) => x + 1); // Noncompliant

            Action a =[SuppressMessage("", "")] () => { }; // Noncompliant

            Action x = true
                           ? ([SuppressMessage("", "")] () => { }) // Noncompliant
                           :[SuppressMessage("", "")] () => { }; // Noncompliant

            Call([SuppressMessage("", "")] (x) => { }); // Noncompliant
        }

        private void Call(Action<int> action) => action(1);
    }

    [UnconditionalSuppressMessage("", "")] // Noncompliant
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    internal class UnconditionalSuppression
    {
        [UnconditionalSuppressMessageAttribute("", "")] // Noncompliant
        public void WithAttributeSuffix() { }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("", "")] // Noncompliant
//                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void QualifiedName() { }

        [CA::UnconditionalSuppressMessage("", "")] // Noncompliant
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void AliasQualifiedName() { }

        [Sys::Obsolete] // Compliant
        public void AliasQualifiedOtherAttribute() { }

        [UnconditionalSuppressMessage("", "", Justification = "Reason")] // Noncompliant
        public void WithJustification() { }

        [UnconditionalSuppressMessage("")] // Compliant // Error [CS7036] - ctor doesn't exist
        public void InvalidConstructor() { }

        public void LocalFunction()
        {
            [UnconditionalSuppressMessage("", "")] int Get() => 1; // Noncompliant
        }
    }
}

class Base(int x) { }
interface IMarker { }

#pragma warning disable A01 // Noncompliant
#pragma warning restore A01 // Compliant
record PositionalRecord(
#pragma warning disable A02 // Noncompliant
    int X)
{
#pragma warning disable A03 // Noncompliant
    public int Y { get; }
#pragma warning disable A04 // Noncompliant
#pragma warning restore A04 // Compliant
}

record PositionalRecordWithSemicolon(int X)
#pragma warning disable A05 // Noncompliant
;

record struct PositionalRecordStruct(int X)
{
#pragma warning disable A06 // Noncompliant
}

class PrimaryConstructor(int x)
#pragma warning disable A07 // Noncompliant
    : Base(
#pragma warning disable A08 // Noncompliant
        x),
    IMarker
#pragma warning disable A09 // Noncompliant
{
#pragma warning disable A10 // Noncompliant
#pragma warning restore A10 // Compliant
}

struct PrimaryConstructorStruct(int x)
{
#pragma warning disable A11 // Noncompliant
}

class WithoutPrimaryConstructor
#pragma warning disable A12 // Noncompliant
    : IMarker
{
#pragma warning disable A13 // Noncompliant
}
